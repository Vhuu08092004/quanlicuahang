using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("WarehouseArea")]
public class WarehouseArea : Quanlicuahang.Models.BasePrimary
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<AreaInventory> AreaInventories { get; set; } = new List<AreaInventory>();
}


