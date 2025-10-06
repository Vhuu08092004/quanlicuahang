using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Quanlicuahang.Enum;

namespace Quanlicuahang.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }
        public List<Role> Role { get; set; } // admin / staff

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        
    }
}
