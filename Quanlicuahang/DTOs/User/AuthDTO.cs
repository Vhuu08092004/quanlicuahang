namespace Quanlicuahang.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string UserId { get; set; } = string.Empty;  
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }

        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
