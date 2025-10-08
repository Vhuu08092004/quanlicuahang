using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("ProductVariant")]
    public class ProductVariant : BasePrimary
    {
        [Required]
        [MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string VariantName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceAdjustment { get; set; } = 0; 

        public int StockQuantity { get; set; } = 0;

        public string? AttributeValueId { get; set; }

        [ForeignKey("AttributeValueId")]
        public virtual ProductAttributeValue? AttributeValue { get; set; }
    }
}
