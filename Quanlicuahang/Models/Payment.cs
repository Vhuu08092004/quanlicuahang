using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Payment")]
    public class Payment : BasePrimary
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "cash";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}
