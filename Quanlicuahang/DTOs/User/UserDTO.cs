using Quanlicuahang.Enum;

namespace Quanlicuahang.DTOs
{
    public class UserRequest
    {
        public string? Username { get; set; } 
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public List<Role> Role { get; set; } = new();
    }

    public class UserUpdateRequest
    {
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public List<Role> Role { get; set; } = new();
    }

    public class UserResponse
    {
        public string? Id { get; set; }         
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public List<Role> Role { get; set; } = new();
    }
}
