using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Data;
using SmartTaskFlow.Models;
using SmartTaskFlow.Filters;

namespace SmartTaskFlow.Controllers
{
    [AuthFilter] // Protection : authentification obligatoire
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Méthode helper pour récupérer l'UserId
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // GET: Task/Index - Liste toutes les tâches de l'utilisateur connecté
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var tasks = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId) // FILTRER PAR UTILISATEUR
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Task/Dashboard - Tableau de bord avec statistiques
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();

            var allTasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.UserId == userId) // FILTRER PAR UTILISATEUR
                .ToListAsync();

            var today = DateTime.Today;
            var viewModel = new
            {
                TotalTasks = allTasks.Count,
                CompletedTasks = allTasks.Count(t => t.Status == "Completed"),
                PendingTasks = allTasks.Count(t => t.Status == "ToDo"),
                InProgressTasks = allTasks.Count(t => t.Status == "InProgress"),
                OverdueTasks = allTasks.Count(t => t.IsOverdue),
                TodayTasks = allTasks.Count(t => t.Deadline?.Date == today),
                Tasks = allTasks
            };

            return View(viewModel);
        }

        // GET: Task/Details/5 - Détails d'une tâche
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var task = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId) // SÉCURITÉ: vérifier que c'est bien sa tâche
                .FirstOrDefaultAsync(m => m.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Task/Create
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();

            // Charger SEULEMENT les catégories de l'utilisateur
            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "CategoryId",
                "CategoryName"
            );
            ViewData["EnergyLevels"] = new SelectList(new[] { "Low", "Medium", "High" });
            ViewData["Priorities"] = new SelectList(new[] { 1, 2, 3, 4, 5 });
            return View();
        }

        // POST: Task/Create - Créer une nouvelle tâche
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("ActivityLogs");

            if (ModelState.IsValid)
            {
                try
                {
                    task.UserId = GetCurrentUserId(); // UTILISER L'UTILISATEUR CONNECTÉ
                    task.CreatedAt = DateTime.Now;
                    task.UpdatedAt = DateTime.Now;
                    task.Status = "ToDo";

                    _context.Add(task);
                    await _context.SaveChangesAsync();

                    // Logger l'activité
                    var log = new UserActivityLog
                    {
                        UserId = task.UserId,
                        TaskId = task.TaskId,
                        Action = "Created",
                        ActionDate = DateTime.Now
                    };
                    _context.UserActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tâche créée avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erreur lors de la création : {ex.Message}");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", task.CategoryId);
            ViewData["EnergyLevels"] = new SelectList(new[] { "Low", "Medium", "High" }, task.EnergyLevel);
            ViewData["Priorities"] = new SelectList(new[] { 1, 2, 3, 4, 5 }, task.Priority);
            return View(task);
        }

        // GET: Task/Edit/5 - Afficher le formulaire de modification
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var task = await _context.Tasks
                .Where(t => t.UserId == userId) // SÉCURITÉ
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            // Toutes les catégories (globales)
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", task.CategoryId);
            ViewData["EnergyLevels"] = new SelectList(new[] { "Low", "Medium", "High" }, task.EnergyLevel);
            ViewData["Priorities"] = new SelectList(new[] { 1, 2, 3, 4, 5 }, task.Priority);
            ViewData["Statuses"] = new SelectList(new[] { "ToDo", "InProgress", "Completed" }, task.Status);
            return View(task);
        }

        // POST: Task/Edit/5 - Modifier une tâche
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {
            if (id != task.TaskId)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            // SÉCURITÉ: Vérifier que la tâche appartient à l'utilisateur
            if (task.UserId != userId)
            {
                return Forbid();
            }

            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("ActivityLogs");

            if (ModelState.IsValid)
            {
                try
                {
                    task.UpdatedAt = DateTime.Now;

                    if (task.Status == "Completed" && task.CompletedAt == null)
                    {
                        task.CompletedAt = DateTime.Now;

                        var log = new UserActivityLog
                        {
                            UserId = task.UserId,
                            TaskId = task.TaskId,
                            Action = "Completed",
                            ActionDate = DateTime.Now
                        };
                        _context.UserActivityLogs.Add(log);
                    }

                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tâche modifiée avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.TaskId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erreur lors de la modification : {ex.Message}");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", task.CategoryId);
            ViewData["EnergyLevels"] = new SelectList(new[] { "Low", "Medium", "High" }, task.EnergyLevel);
            ViewData["Priorities"] = new SelectList(new[] { 1, 2, 3, 4, 5 }, task.Priority);
            ViewData["Statuses"] = new SelectList(new[] { "ToDo", "InProgress", "Completed" }, task.Status);
            return View(task);
        }

        // GET: Task/Delete/5 - Afficher la confirmation de suppression
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var task = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId) // SÉCURITÉ
                .FirstOrDefaultAsync(m => m.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Task/Delete/5 - Supprimer une tâche
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var task = await _context.Tasks
                    .Where(t => t.UserId == userId) // SÉCURITÉ
                    .FirstOrDefaultAsync(t => t.TaskId == id);

                if (task != null)
                {
                    var relatedLogs = await _context.UserActivityLogs
                        .Where(log => log.TaskId == id)
                        .ToListAsync();

                    _context.UserActivityLogs.RemoveRange(relatedLogs);
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tâche supprimée avec succès !";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Task/CompleteTask/5 - Marquer une tâche comme complétée
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var task = await _context.Tasks
                    .Where(t => t.UserId == userId) // SÉCURITÉ
                    .FirstOrDefaultAsync(t => t.TaskId == id);

                if (task == null)
                {
                    return NotFound();
                }

                task.Status = "Completed";
                task.CompletedAt = DateTime.Now;
                task.UpdatedAt = DateTime.Now;

                var log = new UserActivityLog
                {
                    UserId = task.UserId,
                    TaskId = task.TaskId,
                    Action = "Completed",
                    ActionDate = DateTime.Now
                };
                _context.UserActivityLogs.Add(log);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tâche complétée ! 🎉";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Task/SmartSuggestion - Page de sélection
        public IActionResult SmartSuggestion()
        {
            return View();
        }

        // GET: Task/GetSuggestion - Obtenir une suggestion
        public async Task<IActionResult> GetSuggestion(int availableTime, string energyLevel)
        {
            var userId = GetCurrentUserId();

            var suggestedTask = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.UserId == userId  // FILTRER PAR UTILISATEUR
                         && t.Status == "ToDo"
                         && t.EstimatedDuration <= availableTime
                         && t.EnergyLevel == energyLevel)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.Deadline)
                .FirstOrDefaultAsync();

            if (suggestedTask == null)
            {
                TempData["InfoMessage"] = "Aucune tâche ne correspond à vos critères. Essayez avec d'autres paramètres !";
                return RedirectToAction(nameof(SmartSuggestion));
            }

            return View("SmartSuggestion", suggestedTask);
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
    }
}