using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("StockExit")]
    public class StockExit : BasePrimary
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ExitType { get; set; } = "damage"; 

        [Required]
        public DateTime ExitDate { get; set; } = DateTime.UtcNow;

        public string? Reason { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<StockExitItem> StockExitItems { get; set; } = new List<StockExitItem>();
    }
}
