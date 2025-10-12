using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("User")]
    public class User : BasePrimary
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        public string? EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [NotMapped]
        public List<string> Roles => UserRoles.Select(ur => ur.Role.Name).ToList();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
        public virtual ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
        public virtual ICollection<StockExit> StockExits { get; set; } = new List<StockExit>();
    }
}
