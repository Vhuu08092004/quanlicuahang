using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Inventory")]
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Quan hệ
        public Product Product { get; set; }
    }
}
