using Quanlicuahang.Enum;

namespace Quanlicuahang.DTOs.Request
{
    public class UserUpdateRequest
    {
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public List<Role>? Role { get; set; }
    }
}
