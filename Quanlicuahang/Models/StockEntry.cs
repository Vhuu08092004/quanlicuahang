using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("StockEntry")]
    public class StockEntry : BasePrimary
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "pending";

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        public string? Note { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<StockEntryItem> StockEntryItems { get; set; } = new List<StockEntryItem>();
    }
}
