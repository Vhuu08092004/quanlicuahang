using Quanlicuahang.Enum;

namespace Quanlicuahang.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<Role> Role { get; set; } = new();
    }
}
