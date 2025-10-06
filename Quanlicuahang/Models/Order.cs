using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }
        public int? UserId { get; set; }
        public int? PromoId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "pending";

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;

        // Quan hệ
        public Customer Customer { get; set; }
        public User User { get; set; }
        public Promotion Promotion { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
