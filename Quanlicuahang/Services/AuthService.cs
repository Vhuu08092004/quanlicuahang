using Quanlicuahang.Repositories;
using Quanlicuahang.DTOs;
using Quanlicuahang.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Quanlicuahang.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _authRepository.GetUserByUsernameAsync(request.Username);
            if (user == null || user.Password != request.Password)
                return null;

            string token = GenerateJwtToken(user);
            string refreshToken = Guid.NewGuid().ToString(); // Có thể lưu DB nếu muốn

            return new AuthResponse
            {
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            // Ở đây chỉ mô phỏng — thực tế nên lưu refreshToken vào DB
            // để kiểm tra hạn, userId, v.v.
            await Task.CompletedTask;

            // Giả lập user lấy từ refresh token
            var fakeUser = new User
            {
                Username = "refreshedUser",
                FullName = "User Refreshed",
                Role = new List<Enum.Role> { Enum.Role.Staff }
            };

            string newToken = GenerateJwtToken(fakeUser);
            return new AuthResponse
            {
                Username = fakeUser.Username,
                FullName = fakeUser.FullName,
                Role = fakeUser.Role,
                Token = newToken,
                RefreshToken = Guid.NewGuid().ToString()
            };
        }

        public async Task<User?> GetProfileAsync(string username)
        {
            return await _authRepository.GetUserByUsernameAsync(username);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName ?? "")
            };

            foreach (var role in user.Role)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
