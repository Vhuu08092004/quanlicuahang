using Quanlicuahang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ProductVariant")]
public class ProductVariant : BasePrimary
{
    [Required, MaxLength(255)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SKU { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PriceAdjustment { get; set; } = 0;

    public int StockQuantity { get; set; } = 0;

    public virtual ICollection<ProductVariantAttributeValue> VariantValues { get; set; } = new List<ProductVariantAttributeValue>();
}
