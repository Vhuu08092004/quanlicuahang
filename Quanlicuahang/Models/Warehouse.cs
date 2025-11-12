using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Warehouse")]
public class Warehouse : Quanlicuahang.Models.BasePrimary
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public virtual ICollection<WarehouseArea> Areas { get; set; } = new List<WarehouseArea>();
}


