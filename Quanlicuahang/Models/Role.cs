using Quanlicuahang.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Role")]
    public class Role : BasePrimary
    {
        [Required]
        [MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Permission Permissions { get; set; } = Permission.None;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
