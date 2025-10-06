using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; }

        [MaxLength(50)]
        public string Barcode { get; set; }

        [Required]
        public decimal Price { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "pcs";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public Inventory Inventory { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
