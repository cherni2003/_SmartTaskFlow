using System.ComponentModel.DataAnnotations;

namespace SmartTaskFlow.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [StringLength(50, ErrorMessage = "Maximum 50 caractères")]
        public string Username { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User"; // "User" ou "Admin"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        // Navigation property
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
    }
}