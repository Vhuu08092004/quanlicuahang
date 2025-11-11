using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quanlicuahang.Models
{
    [Table("Employee")]
    public class Employee : BasePrimary
    {
        // ✅ Cột mã nhân viên
        [Required]
        [MaxLength(50)]
        [Column("Code")]
        public string Code { get; set; } = string.Empty;

        // Ánh xạ sang cột FullName trong database
        [Required]
        [MaxLength(100)]
        [Column("FullName")]
        public string Name { get; set; } = string.Empty;

        // Ánh xạ sang cột PhoneNumber
        [MaxLength(20)]
        [Column("PhoneNumber")]
        public string Phone { get; set; } = string.Empty;

        // Email giữ nguyên
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Địa chỉ
        public string Address { get; set; } = string.Empty;

        // Ngày sinh
        [Column("BirthDate")]
        public DateTime? BirthDate { get; set; }

        // Chức vụ
        [MaxLength(100)]
        [Column("Position")]
        public string? Position { get; set; }

        // Thuộc tính tạm không lưu vào DB
        [NotMapped]
        public string FullName => Name;

        // Quan hệ với User
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
