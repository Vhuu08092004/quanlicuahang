using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Customer")]
    public class Customer : BasePrimary
    {

        [Required]
        [MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
