using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Data;
using SmartTaskFlow.Models;
using SmartTaskFlow.Filters;

namespace SmartTaskFlow.Controllers
{
    [AuthFilter]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper : récupérer l'ID utilisateur
        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        // Helper : vérifier si l'utilisateur est admin
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // Helper : redirection sécurisée en cas d'accès non autorisé
        private IActionResult UnauthorizedRedirect(string message = "Vous n'êtes pas autorisé à accéder à cette page.")
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction("Index", "Home");
        }

        // GET: Category/Index
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            IQueryable<Category> query = _context.Categories.Include(c => c.Tasks).Include(c => c.User);

            if (!IsAdmin())
            {
                query = query.Where(c => c.UserId == userId);
            }

            var categories = await query.OrderBy(c => c.CategoryName).ToListAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetCurrentUserId();
            Category? category;

            if (IsAdmin())
            {
                category = await _context.Categories
                    .Include(c => c.Tasks)
                    .ThenInclude(t => t.User)
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
            }
            else
            {
                category = await _context.Categories
                    .Include(c => c.Tasks)
                    .ThenInclude(t => t.User)
                    .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
            }

            if (category == null) return UnauthorizedRedirect("Catégorie introuvable ou accès refusé.");
            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create() => View();

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ModelState.Remove("Tasks");
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (string.IsNullOrWhiteSpace(category.ColorCode))
                category.ColorCode = "#3498db";

            if (ModelState.IsValid)
            {
                try
                {
                    category.UserId = GetCurrentUserId();
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Catégorie '{category.CategoryName}' créée avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erreur : {ex.Message}");
                }
            }

            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await GetCategoryForCurrentUser(id.Value);
            if (category == null) return UnauthorizedRedirect();

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId) return NotFound();

            var categoryInDb = await GetCategoryForCurrentUser(id);
            if (categoryInDb == null) return UnauthorizedRedirect();

            categoryInDb.CategoryName = category.CategoryName;
            categoryInDb.ColorCode = string.IsNullOrWhiteSpace(category.ColorCode) ? "#3498db" : category.ColorCode;
            categoryInDb.Icon = category.Icon;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Catégorie '{categoryInDb.CategoryName}' modifiée avec succès !";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur : {ex.Message}");
            }

            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await GetCategoryForCurrentUser(id.Value);
            if (category == null) return UnauthorizedRedirect();

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await GetCategoryForCurrentUser(id);
            if (category == null) return UnauthorizedRedirect();

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Catégorie '{category.CategoryName}' supprimée avec succès !";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper pour récupérer la catégorie selon l'utilisateur ou admin
        private async Task<Category?> GetCategoryForCurrentUser(int categoryId)
        {
            var userId = GetCurrentUserId();

            if (IsAdmin())
            {
                return await _context.Categories
                    .Include(c => c.Tasks)
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            }
            else
            {
                return await _context.Categories
                    .Include(c => c.Tasks)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);
            }
        }

        private bool CategoryExists(int id)
        {
            var userId = GetCurrentUserId();
            if (IsAdmin())
                return _context.Categories.Any(c => c.CategoryId == id);
            else
                return _context.Categories.Any(c => c.CategoryId == id && c.UserId == userId);
        }
    }
}
