using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });
            var user = await _authService.GetProfileByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng!" });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.FullName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Đăng xuất thành công!" });
        }
    }
}
