using Quanlicuahang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ProductAttributeValue")]
public class ProductAttributeValue : BasePrimary
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;

    [ForeignKey(nameof(AttributeId))]
    public virtual ProductAttribute Attribute { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Value { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public virtual ICollection<ProductVariantAttributeValue> VariantValues { get; set; } = new List<ProductVariantAttributeValue>();
}
