using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("OrderItem")]
    public class OrderItem : BasePrimary
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [Required]
        public string ProductId { get; set; } = string.Empty;
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}
