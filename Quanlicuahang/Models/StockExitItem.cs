using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("StockExitItem")]
    public class StockExitItem : BasePrimary
    {
        [Required]
        public string StockExitId { get; set; } = string.Empty;

        [ForeignKey("StockExitId")]
        public virtual StockExit StockExit { get; set; } = null!;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        public string? Note { get; set; }
    }
}
