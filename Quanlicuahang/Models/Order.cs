using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Quanlicuahang.Enum;

namespace Quanlicuahang.Models
{
    [Table("Order")]
    public class Order : BasePrimary
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public string? PromoId { get; set; }
        [ForeignKey("PromoId")]
        public virtual Promotion? Promotion { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal RemainingAmount => TotalAmount - DiscountAmount - PaidAmount;

        public bool IsFullyPaid => PaidAmount >= (TotalAmount - DiscountAmount);

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
