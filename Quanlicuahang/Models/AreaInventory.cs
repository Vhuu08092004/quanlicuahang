using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AreaInventory")]
public class AreaInventory : Quanlicuahang.Models.BasePrimary
{
    [Required]
    public string WarehouseAreaId { get; set; } = string.Empty;

    [ForeignKey(nameof(WarehouseAreaId))]
    public virtual WarehouseArea WarehouseArea { get; set; } = null!;

    [Required]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}


