using Quanlicuahang.Enum;

namespace Quanlicuahang.DTOs.Request
{
    public class UserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public List<Role>? Role { get; set; }
    }
}
