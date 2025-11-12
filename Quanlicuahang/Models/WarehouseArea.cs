using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("WarehouseArea")]
public class WarehouseArea : Quanlicuahang.Models.BasePrimary
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? WarehouseId { get; set; }

    [ForeignKey(nameof(WarehouseId))]
    public virtual Warehouse? Warehouse { get; set; }

    public virtual ICollection<AreaInventory> AreaInventories { get; set; } = new List<AreaInventory>();
}


