using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskFlow.Models
{
    public class UserActivityLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? TaskId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } // "Created", "Completed", "Deleted"

        public DateTime ActionDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("TaskId")]
        public TaskItem? Task { get; set; }
    }
}