using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskFlow.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        // Lier la catégorie à un utilisateur
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
        [StringLength(50, ErrorMessage = "Maximum 50 caractères")]
        [Display(Name = "Nom de la catégorie")]
        public string CategoryName { get; set; }

        [StringLength(7)] // Format: #FF5733
        [Display(Name = "Code couleur")]
        public string? ColorCode { get; set; }

        [StringLength(50)] // Emoji ou icône
        [Display(Name = "Icône")]
        public string? Icon { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}