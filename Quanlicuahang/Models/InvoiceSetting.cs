using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("InvoiceSetting")]
    public class InvoiceSetting : BasePrimary
    {
        [MaxLength(255)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string StoreAddress { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;
    }
}

