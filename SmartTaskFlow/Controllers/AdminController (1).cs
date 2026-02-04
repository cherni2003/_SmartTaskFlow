using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Data;
using SmartTaskFlow.Models;
using SmartTaskFlow.Filters;

namespace SmartTaskFlow.Controllers
{
    [AuthFilter] // Protection : authentification obligatoire
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Méthode helper pour récupérer l'UserId
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // Méthode helper pour vérifier si l'utilisateur est admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Admin/Index - Tableau de bord administrateur
        public async Task<IActionResult> Index()
        {
            // Vérifier que l'utilisateur est admin
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé. Cette page est réservée aux administrateurs.";
                return RedirectToAction("Index", "Task");
            }

            // Statistiques globales
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalTasks = await _context.Tasks.CountAsync();
            var completedTasks = await _context.Tasks.CountAsync(t => t.Status == "Completed");
            var totalCategories = await _context.Categories.CountAsync();

            // Activités récentes (les 10 dernières)
            var recentActivities = await _context.UserActivityLogs
                .Include(log => log.User)
                .Include(log => log.Task)
                .OrderByDescending(log => log.ActionDate)
                .Take(10)
                .ToListAsync();

            var viewModel = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = totalTasks - completedTasks,
                TotalCategories = totalCategories,
                RecentActivities = recentActivities
            };

            return View(viewModel);
        }

        // GET: Admin/Users - Gestion des utilisateurs
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            var users = await _context.Users
                .Select(u => new
                {
                    User = u,
                    TaskCount = u.Tasks.Count,
                    CompletedTaskCount = u.Tasks.Count(t => t.Status == "Completed")
                })
                .ToListAsync();

            // Créer un ViewBag avec les statistiques
            ViewBag.TaskCounts = users.ToDictionary(x => x.User.UserId, x => x.TaskCount);
            ViewBag.CompletedCounts = users.ToDictionary(x => x.User.UserId, x => x.CompletedTaskCount);

            return View(users.Select(x => x.User).OrderBy(u => u.Username).ToList());
        }

        // GET: Admin/UserDetails/5 - Détails d'un utilisateur
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Tasks)
                    .ThenInclude(t => t.Category)
                .Include(u => u.ActivityLogs)
                    .ThenInclude(log => log.Task)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/ToggleUserStatus/5 - Activer/Désactiver un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Ne pas permettre de se désactiver soi-même
                if (user.UserId == GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "Vous ne pouvez pas désactiver votre propre compte.";
                    return RedirectToAction(nameof(Users));
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                var status = user.IsActive ? "activé" : "désactivé";
                TempData["SuccessMessage"] = $"Utilisateur '{user.Username}' {status} avec succès !";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur : {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ChangeUserRole/5 - Changer le rôle d'un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(int id, string newRole)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Ne pas permettre de changer son propre rôle
                if (user.UserId == GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "Vous ne pouvez pas modifier votre propre rôle.";
                    return RedirectToAction(nameof(Users));
                }

                // Valider le rôle
                if (newRole != "Admin" && newRole != "User")
                {
                    TempData["ErrorMessage"] = "Rôle invalide.";
                    return RedirectToAction(nameof(Users));
                }

                user.Role = newRole;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Rôle de '{user.Username}' changé en '{newRole}' avec succès !";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur : {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/DeleteUser/5 - Confirmation de suppression d'utilisateur
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Ne pas permettre de se supprimer soi-même
            if (user.UserId == GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "Vous ne pouvez pas supprimer votre propre compte.";
                return RedirectToAction(nameof(Users));
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5 - Supprimer un utilisateur
        // POST: Admin/DeleteUser/5 - Supprimer un utilisateur
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            try
            {
                // Charger l'utilisateur avec toutes ses dépendances
                var user = await _context.Users
                    .Include(u => u.Tasks)
                    .Include(u => u.ActivityLogs)
                    .Include(u => u.Categories)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound();
                }

                // Ne pas permettre de se supprimer soi-même
                if (user.UserId == GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "Vous ne pouvez pas supprimer votre propre compte.";
                    return RedirectToAction(nameof(Users));
                }

                // Supprimer les tâches associées
                if (user.Tasks.Any())
                    _context.Tasks.RemoveRange(user.Tasks);

                // Supprimer les logs d'activité associés
                if (user.ActivityLogs.Any())
                    _context.UserActivityLogs.RemoveRange(user.ActivityLogs);

                // Supprimer les catégories associées
                if (user.Categories.Any())
                    _context.Categories.RemoveRange(user.Categories);

                // Supprimer l'utilisateur
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Utilisateur '{user.Username}' et toutes ses données ont été supprimés avec succès !";
            }
            catch (DbUpdateException dbEx)
            {
                // Cette exception capture les violations de contraintes
                TempData["ErrorMessage"] = $"Impossible de supprimer l'utilisateur car il est lié à d'autres données : {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }


        // GET: Admin/AllTasks - Voir toutes les tâches de tous les utilisateurs
        public async Task<IActionResult> AllTasks()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            var tasks = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Admin/AllCategories - Voir toutes les catégories
        public async Task<IActionResult> AllCategories()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            var categories = await _context.Categories
                .Include(c => c.User) // IMPORTANT : Inclure l'utilisateur
                .Include(c => c.Tasks)
                .Select(c => new
                {
                    Category = c,
                    TaskCount = c.Tasks.Count,
                    UserName = c.User.Username // Nom de l'utilisateur propriétaire
                })
                .OrderBy(c => c.UserName)
                .ThenBy(c => c.Category.CategoryName)
                .ToListAsync();

            ViewBag.TaskCounts = categories.ToDictionary(
                x => x.Category.CategoryId,
                x => x.TaskCount
            );

            ViewBag.UserNames = categories.ToDictionary(
                x => x.Category.CategoryId,
                x => x.UserName
            );

            return View(categories.Select(x => x.Category).ToList());
        }

        // GET: Admin/ActivityLogs - Journal d'activité complet
        public async Task<IActionResult> ActivityLogs(int? userId, DateTime? startDate, DateTime? endDate)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            var query = _context.UserActivityLogs
                .Include(log => log.User)
                .Include(log => log.Task)
                .AsQueryable();

            // Filtrer par utilisateur si spécifié
            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(log => log.UserId == userId.Value);
            }

            // Filtrer par date de début
            if (startDate.HasValue)
            {
                query = query.Where(log => log.ActionDate >= startDate.Value);
            }

            // Filtrer par date de fin
            if (endDate.HasValue)
            {
                query = query.Where(log => log.ActionDate <= endDate.Value.AddDays(1));
            }

            var logs = await query
                .OrderByDescending(log => log.ActionDate)
                .Take(100) // Limiter à 100 entrées pour la performance
                .ToListAsync();

            // Passer les utilisateurs pour le filtre
            ViewBag.Users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.SelectedUserId = userId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(logs);
        }

        // GET: Admin/Statistics - Statistiques détaillées
        public async Task<IActionResult> Statistics()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Task");
            }

            // Charger les utilisateurs et leurs tâches en mémoire
            var users = await _context.Users
                .Include(u => u.Tasks)
                .ToListAsync();

            // Statistiques par utilisateur (calculées en mémoire)
            var userStats = users
                .Select(u => new
                {
                    User = u,
                    TotalTasks = u.Tasks.Count,
                    CompletedTasks = u.Tasks.Count(t => t.Status == "Completed"),
                    PendingTasks = u.Tasks.Count(t => t.Status == "ToDo"),
                    InProgressTasks = u.Tasks.Count(t => t.Status == "InProgress"),
                    OverdueTasks = u.Tasks.Count(t => t.IsOverdue)
                })
                .ToList();

            // Statistiques par catégorie (AVEC le nom d'utilisateur)
            var categoryStats = await _context.Categories
                .Include(c => c.User)
                .Select(c => new
                {
                    Category = c,
                    UserName = c.User.Username,
                    TotalTasks = c.Tasks.Count,
                    CompletedTasks = c.Tasks.Count(t => t.Status == "Completed")
                })
                .OrderBy(c => c.UserName)
                .ThenByDescending(c => c.TotalTasks)
                .ToListAsync();

            // Tâches créées par jour (7 derniers jours)
            var today = DateTime.Today;
            var tasksPerDay = new Dictionary<string, int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.Tasks
                    .CountAsync(t => t.CreatedAt.Date == date);
                tasksPerDay.Add(date.ToString("dd/MM"), count);
            }

            var viewModel = new
            {
                UserStats = userStats,
                CategoryStats = categoryStats,
                TasksPerDay = tasksPerDay
            };

            return View(viewModel);
        }
    }
}