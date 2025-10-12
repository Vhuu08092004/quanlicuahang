using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Employee")]
    public class Employee : BasePrimary
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
