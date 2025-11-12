using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("StockEntryItem")]
    public class StockEntryItem : BasePrimary
    {
        [Required]
        public string StockEntryId { get; set; } = string.Empty;

        [ForeignKey("StockEntryId")]
        public virtual StockEntry StockEntry { get; set; } = null!;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        public string? WarehouseAreaId { get; set; }

        [ForeignKey("WarehouseAreaId")]
        public virtual WarehouseArea? WarehouseArea { get; set; }
    }

}
