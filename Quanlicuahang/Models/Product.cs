using Quanlicuahang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Product")]
public class Product : BasePrimary
{
    [Required, MaxLength(255)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "pcs";

    public int Quantity { get; set; } = 0;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public string? CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    public string? SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    public virtual ICollection<StockEntryItem> StockEntryItems { get; set; } = new List<StockEntryItem>();
    public virtual ICollection<StockExitItem> StockExitItems { get; set; } = new List<StockExitItem>();

    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
}
