using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("UserRole")]
    public class UserRole : BasePrimary
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
