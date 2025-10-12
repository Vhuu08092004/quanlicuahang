using Quanlicuahang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ProductVariantAttributeValue")]
public class ProductVariantAttributeValue : BasePrimary
{
    [Required]
    public string ProductVariantId { get; set; } = string.Empty;

    [Required]
    public string AttributeValueId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductVariantId))]
    public virtual ProductVariant ProductVariant { get; set; } = null!;

    [ForeignKey(nameof(AttributeValueId))]
    public virtual ProductAttributeValue AttributeValue { get; set; } = null!;
}
