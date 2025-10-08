using Mysqlx.Crud;
using Quanlicuahang.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("User")]
    public class User : BasePrimary
    {
        

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(20)]
        public List<Role> Role { get; set; }  // admin / staff

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
