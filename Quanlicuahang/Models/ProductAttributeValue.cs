using Quanlicuahang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ProductAttributeValue")]
public class ProductAttributeValue : BasePrimary
{
    // Liên kết đến Product
    [Required]
    public string ProductId { get; set; } = string.Empty;
    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    // Liên kết đến Attribute
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    [ForeignKey(nameof(AttributeId))]
    public virtual ProductAttribute Attribute { get; set; } = null!;

    [MaxLength(255)]
    public string? ValueString { get; set; }

    public decimal? ValueDecimal { get; set; }

    public int? ValueInt { get; set; }

    public bool? ValueBool { get; set; }

    public DateTime? ValueDate { get; set; }

    public int DisplayOrder { get; set; } = 0;
}
