using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskFlow.Models
{
    // On appelle la classe "TaskItem" au lieu de "Task" 
    // pour éviter le conflit avec System.Threading.Tasks.Task
    [Table("Tasks")]
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(200, ErrorMessage = "Maximum 200 caractères")]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [StringLength(2000)]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        // ============================================
        // Caractéristiques intelligentes
        // ============================================

        [Required(ErrorMessage = "La durée estimée est obligatoire")]
        [Range(5, 480, ErrorMessage = "Entre 5 minutes et 8 heures (480 min)")]
        [Display(Name = "Durée estimée (minutes)")]
        public int EstimatedDuration { get; set; } // en minutes

        [Required(ErrorMessage = "Le niveau d'énergie est obligatoire")]
        [StringLength(20)]
        [Display(Name = "Niveau d'énergie")]
        public string EnergyLevel { get; set; } // "Low", "Medium", "High"

        [Required(ErrorMessage = "La priorité est obligatoire")]
        [Range(1, 5, ErrorMessage = "Entre 1 (faible) et 5 (urgent)")]
        [Display(Name = "Priorité")]
        public int Priority { get; set; }

        // ============================================
        // Statut et dates
        // ============================================

        [Required]
        [StringLength(20)]
        [Display(Name = "Statut")]
        public string Status { get; set; } = "ToDo"; // "ToDo", "InProgress", "Completed"

        [Display(Name = "Date limite")]
        [DataType(DataType.DateTime)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Complétée le")]
        [DataType(DataType.DateTime)]
        public DateTime? CompletedAt { get; set; }

        [Display(Name = "Créée le")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Mise à jour le")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ============================================
        // Navigation properties (relations)
        // ============================================

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();

        // ============================================
        // Propriétés calculées (non stockées en DB)
        // ============================================

        [NotMapped]
        [Display(Name = "En retard ?")]
        public bool IsOverdue => Deadline.HasValue &&
                                  Deadline.Value < DateTime.Now &&
                                  Status != "Completed";

        [NotMapped]
        [Display(Name = "Durée formatée")]
        public string FormattedDuration
        {
            get
            {
                if (EstimatedDuration < 60)
                    return $"{EstimatedDuration} min";
                else
                {
                    int hours = EstimatedDuration / 60;
                    int minutes = EstimatedDuration % 60;
                    return minutes > 0 ? $"{hours}h {minutes}min" : $"{hours}h";
                }
            }
        }

        [NotMapped]
        [Display(Name = "Niveau d'énergie (FR)")]
        public string EnergyLevelFrench
        {
            get
            {
                return EnergyLevel switch
                {
                    "Low" => "Faible",
                    "Medium" => "Moyen",
                    "High" => "Élevé",
                    _ => EnergyLevel
                };
            }
        }

        [NotMapped]
        [Display(Name = "Statut (FR)")]
        public string StatusFrench
        {
            get
            {
                return Status switch
                {
                    "ToDo" => "À faire",
                    "InProgress" => "En cours",
                    "Completed" => "Terminée",
                    _ => Status
                };
            }
        }

        [NotMapped]
        [Display(Name = "Badge priorité")]
        public string PriorityBadge
        {
            get
            {
                return Priority switch
                {
                    5 => "🔴 Urgent",
                    4 => "🟠 Élevée",
                    3 => "🟡 Moyenne",
                    2 => "🟢 Faible",
                    1 => "⚪ Très faible",
                    _ => "❓"
                };
            }
        }
    }
}