using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{

    [Table("ActionLog")]
    public class ActionLog : BasePrimary
    {
        [Required, MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Action { get; set; }

        [Required, MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        public string? Description { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}