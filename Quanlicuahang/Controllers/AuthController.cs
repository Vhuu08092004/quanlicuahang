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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "Đăng ký thất bại. Username hoặc Email đã tồn tại, hoặc dữ liệu không hợp lệ."
                });
            }

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var ok = await _authService.SendResetPasswordOtpAsync(request);

            if (!ok)
                return BadRequest(new { message = "Yêu cầu đặt lại mật khẩu không hợp lệ." });

            // Không tiết lộ email có tồn tại hay không
            return Ok(new { message = "Nếu email tồn tại, mã OTP sẽ được gửi để đặt lại mật khẩu." });
        }


          // ⭐ ĐẶT LẠI MẬT KHẨU BẰNG OTP
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var ok = await _authService.ResetPasswordAsync(request);

            if (!ok)
                return BadRequest(new { message = "Không thể đặt lại mật khẩu. Vui lòng kiểm tra lại email, OTP và mật khẩu." });

            return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
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
