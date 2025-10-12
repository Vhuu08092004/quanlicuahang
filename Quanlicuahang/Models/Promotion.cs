using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Promotion")]
    public class Promotion : BasePrimary
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required, MaxLength(20)]
        public string DiscountType { get; set; } = "percent";

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        public int UsageLimit { get; set; } = 0;
        public int UsedCount { get; set; } = 0;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "active";

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
