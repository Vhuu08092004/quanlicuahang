using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Promotion")]
    public class Promotion
    {
        [Key]
        public int PromoId { get; set; }

        [Required, MaxLength(50)]
        public string PromoCode { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public string DiscountType { get; set; } // percent / fixed

        [Required]
        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal MinOrderAmount { get; set; } = 0;
        public int UsageLimit { get; set; } = 0;
        public int UsedCount { get; set; } = 0;

        public string Status { get; set; } = "active";

        // Quan hệ
        public ICollection<Order> Orders { get; set; }
    }
}
