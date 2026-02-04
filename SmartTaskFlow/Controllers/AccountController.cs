using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Data;
using SmartTaskFlow.Models;
using BCrypt.Net;

namespace SmartTaskFlow.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Si déjà connecté, rediriger vers les tâches
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Task");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Veuillez remplir tous les champs.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Nom d'utilisateur ou mot de passe incorrect.";
                return View();
            }

            // Vérifier le mot de passe
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                TempData["ErrorMessage"] = "Nom d'utilisateur ou mot de passe incorrect.";
                return View();
            }

            // Créer la session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            // Mettre à jour la date de dernière connexion
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bienvenue {user.FullName} ! 🎉";
            return RedirectToAction("Index", "Task");
        }

        // GET: Account/Signup
        public IActionResult Signup()
        {
            // Si déjà connecté, rediriger vers les tâches
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Task");
            }
            return View();
        }

        // POST: Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(string username, string email, string password, string confirmPassword, string fullName)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ErrorMessage"] = "Tous les champs sont obligatoires.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Les mots de passe ne correspondent pas.";
                return View();
            }

            if (password.Length < 6)
            {
                TempData["ErrorMessage"] = "Le mot de passe doit contenir au moins 6 caractères.";
                return View();
            }

            // Vérifier si l'utilisateur existe déjà
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

            if (existingUser != null)
            {
                if (existingUser.Username == username)
                {
                    TempData["ErrorMessage"] = "Ce nom d'utilisateur est déjà utilisé.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cet email est déjà utilisé.";
                }
                return View();
            }

            try
            {
                // Créer le nouvel utilisateur
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FullName = fullName,
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Créer des catégories par défaut pour le nouvel utilisateur
                var defaultCategories = new List<Category>
                {
                    new Category { UserId = newUser.UserId, CategoryName = "Travail", ColorCode = "#3498db", Icon = "💼" },
                    new Category { UserId = newUser.UserId, CategoryName = "Études", ColorCode = "#e74c3c", Icon = "📚" },
                    new Category { UserId = newUser.UserId, CategoryName = "Personnel", ColorCode = "#2ecc71", Icon = "🏠" },
                    new Category { UserId = newUser.UserId, CategoryName = "Sport", ColorCode = "#f39c12", Icon = "🏋️" },
                    new Category { UserId = newUser.UserId, CategoryName = "Santé", ColorCode = "#9b59b6", Icon = "❤️" }
                };

                _context.Categories.AddRange(defaultCategories);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Une erreur est survenue : {ex.Message}";
                return View();
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Vous êtes déconnecté. À bientôt ! 👋";
            return RedirectToAction("Login");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}