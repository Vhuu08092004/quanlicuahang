using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("ReturnItem")]
    public class ReturnItem : BasePrimary
    {
        [Required]
        public string ReturnId { get; set; } = string.Empty;

        [ForeignKey("ReturnId")]
        public virtual Return Return { get; set; } = null!;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal RefundPrice { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
