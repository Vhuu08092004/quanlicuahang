using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("UserToken")]
    public class UserToken : BasePrimary
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime Expiration { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [MaxLength(50)]
        public string Type { get; set; } = "RefreshToken";

        public bool IsRevoked { get; set; } = false;
    }
}
