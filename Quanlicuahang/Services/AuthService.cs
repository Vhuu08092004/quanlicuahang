using Microsoft.IdentityModel.Tokens;
using Quanlicuahang.DTOs;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Quanlicuahang.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            IAuthRepository authRepository,
            IUserRepository userRepository,
            IUserTokenRepository userTokenRepository,
            IActionLogService logService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _emailService = emailService;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            try
            {
                var user = await _authRepository.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "Login",
                        entityType: "Auth",
                        entityId: request.Username,
                        description: $"Login thất bại - User '{request.Username}' không tồn tại",
                        oldValue: null,
                        newValue: null,
                        userId: "Admin",
                        ip: ip,
                        userAgent: agent
                    );
                    return null;
                }

                if (user.IsDeleted)
                {
                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "Login",
                        entityType: "Auth",
                        entityId: user.Id,
                        description: $"Login thất bại - User '{user.Username}' đã bị vô hiệu hóa",
                        oldValue: null,
                        newValue: null,
                        userId: user.Id,
                        ip: ip,
                        userAgent: agent
                    );
                    return null;
                }

                var hashedPassword = HashPassword(request.Password);
                if (user.Password != hashedPassword)
                {
                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "Login",
                        entityType: "Auth",
                        entityId: user.Id,
                        description: $"Login thất bại - Mật khẩu sai cho user '{user.Username}'",
                        oldValue: null,
                        newValue: null,
                        userId: user.Id,
                        ip: ip,
                        userAgent: agent
                    );
                    return null;
                }

                string jwtToken = GenerateJwtToken(user);
                string refreshToken = Guid.NewGuid().ToString();

                var userToken = new UserToken
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    Token = refreshToken,
                    Expiration = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _userTokenRepository.AddTokenAsync(userToken);

                var userIdFromToken = await _userTokenRepository.GetUserIdByTokenAsync(refreshToken);

                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "Login",
                    entityType: "Auth",
                    entityId: user.Id,
                    description: $"Login thành công - User '{user.Username}'",
                    oldValue: null,
                    newValue: new { user.Id, user.Username, loginTime = DateTime.UtcNow },
                    userId: userIdFromToken ?? user.Id,
                    ip: ip,
                    userAgent: agent
                );

                return new AuthResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Permissions = user.UserRoles
                        .Select(ur => ur.Role.Permissions)
                        .Aggregate(Quanlicuahang.Enum.Permission.None, (current, next) => current | next)
                        .ToString()
                        .Split(", ")
                        .Where(p => p != "None")
                        .ToList(),
                    Token = jwtToken,
                    RefreshToken = refreshToken
                };
            }
            catch (System.Exception ex)
            {
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "Login",
                    entityType: "Auth",
                    entityId: request.Username,
                    description: $"Login exception - {ex.Message}",
                    oldValue: null,
                    newValue: null,
                    userId: "SYSTEM",
                    ip: ip,
                    userAgent: agent
                );
                throw;
            }
        }
        public async Task<bool?> RegisterAsync(RegisterRequest request)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            try
            {
                // 1. Validate cơ bản
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return false;
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return false;
                }

                // 2. Check trùng username
                var existedUser = await _authRepository.GetUserByUsernameAsync(request.Username);
                if (existedUser != null)
                {
                    return false;
                }

                // 3. Check trùng email
                var existedEmailUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existedEmailUser != null)
                {
                    return false;
                }

                // 4. Hash password
                var hashedPassword = HashPassword(request.Password);

                // 5. Tạo user mới
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    Password = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _userRepository.AddAsync(newUser);

                // 6. Log hoạt động đăng ký
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "Register",
                    entityType: "Auth",
                    entityId: newUser.Id,
                    description: $"User '{newUser.Username}' đăng ký tài khoản thành công",
                    oldValue: null,
                    newValue: newUser,
                    userId: newUser.Id,
                    ip: ip,
                    userAgent: agent
                );

                return true;
            }
            catch (System.Exception ex)
            {
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "RegisterException",
                    entityType: "Auth",
                    entityId: request.Username,
                    description: $"Register exception - {ex.Message}",
                    oldValue: null,
                    newValue: null,
                    userId: "SYSTEM",
                    ip: ip,
                    userAgent: agent
                );

                throw;
            }
        }

        public async Task<bool> SendResetPasswordOtpAsync(ForgotPasswordRequest request)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return false;
                }

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "ForgotPassword",
                        entityType: "Auth",
                        entityId: request.Email,
                        description: $"Yêu cầu quên mật khẩu với email không tồn tại: {request.Email}",
                        oldValue: null,
                        newValue: null,
                        userId: "SYSTEM",
                        ip: ip,
                        userAgent: agent
                    );

                    // vẫn trả true để không lộ email tồn tại hay không
                    return true;
                }

                // Tạo mã OTP 6 chữ số
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                var token = new UserToken
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    Token = otpCode,
                    Expiration = DateTime.UtcNow.AddMinutes(10),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _userTokenRepository.AddTokenAsync(token);

                var subject = "Reset password OTP";
                var body = $"Your OTP code is: {otpCode}\nThis code will expire in 10 minutes.";

                await _emailService.SendEmailAsync(user.Email, subject, body);

                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "SendResetPasswordOtp",
                    entityType: "Auth",
                    entityId: user.Id,
                    description: $"Gửi OTP reset mật khẩu tới email {request.Email}",
                    oldValue: null,
                    newValue: new { Email = request.Email },
                    userId: user.Id,
                    ip: ip,
                    userAgent: agent
                );

                return true;
            }
            catch (System.Exception ex)
            {
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "SendResetPasswordOtpException",
                    entityType: "Auth",
                    entityId: request.Email,
                    description: $"Lỗi gửi OTP reset mật khẩu - {ex.Message}",
                    oldValue: null,
                    newValue: null,
                    userId: "SYSTEM",
                    ip: ip,
                    userAgent: agent
                );
                throw;
            }
        }

        // ⭐ ĐẶT LẠI MẬT KHẨU BẰNG OTP
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.OtpCode) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return false;
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return false;
                }

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return false;
                }

                var token = await _userTokenRepository.GetTokenAsync(request.OtpCode);
                if (token == null ||
                    token.IsRevoked ||
                    token.Expiration < DateTime.UtcNow ||
                    token.UserId != user.Id)
                {
                    return false;
                }

                user.Password = HashPassword(request.NewPassword);
                await _userRepository.UpdateAsync(user);

                token.IsRevoked = true;
                await _userTokenRepository.UpdateTokenAsync(token);

                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "ResetPassword",
                    entityType: "Auth",
                    entityId: user.Id,
                    description: $"User '{user.Username}' reset mật khẩu thành công bằng OTP",
                    oldValue: null,
                    newValue: new { user.Id, user.Username, resetTime = DateTime.UtcNow },
                    userId: user.Id,
                    ip: ip,
                    userAgent: agent
                );

                return true;
            }
            catch (System.Exception ex)
            {
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "ResetPasswordException",
                    entityType: "Auth",
                    entityId: request.Email,
                    description: $"Lỗi reset mật khẩu - {ex.Message}",
                    oldValue: null,
                    newValue: null,
                    userId: "SYSTEM",
                    ip: ip,
                    userAgent: agent
                );
                throw;
            }
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var existingToken = await _userTokenRepository.GetTokenAsync(refreshToken);
            if (existingToken == null || existingToken.Expiration < DateTime.UtcNow || existingToken.IsRevoked)
                return null;

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user == null)
                return null;

            existingToken.IsRevoked = true;
            await _userTokenRepository.UpdateTokenAsync(existingToken);
            string newRefreshToken = Guid.NewGuid().ToString();
            var newTokenEntity = new UserToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Token = newRefreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            await _userTokenRepository.AddTokenAsync(newTokenEntity);
            var userIdFromToken = await _userTokenRepository.GetUserIdByTokenAsync(newRefreshToken);

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "RefreshToken",
                entityType: "Auth",
                entityId: user.Id,
                description: $"Refresh token thành công - User '{user.Username}'",
                oldValue: null,
                newValue: new { user.Id, user.Username, refreshTime = DateTime.UtcNow },
                userId: userIdFromToken ?? user.Id,
                ip: ip,
                userAgent: agent
            );

            string newJwt = GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Permissions = user.UserRoles
                    .Select(ur => ur.Role.Permissions)
                    .Aggregate(Quanlicuahang.Enum.Permission.None, (current, next) => current | next)
                    .ToString()
                    .Split(", ")
                    .Where(p => p != "None")
                    .ToList(),
                Token = newJwt,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<User?> GetProfileAsync(string username)
        {
            return await _authRepository.GetUserByUsernameAsync(username);
        }

        public async Task<User?> GetProfileByIdAsync(string userId)
        {
            return await _userRepository.GetByIdWithRolesAsync(userId);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName ?? "")
            };

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var allPermissions = user.UserRoles
                .Select(ur => ur.Role.Permissions)
                .Aggregate(Quanlicuahang.Enum.Permission.None, (current, next) => current | next);

            var permissionNames = System.Enum.GetValues(typeof(Quanlicuahang.Enum.Permission))
                .Cast<Quanlicuahang.Enum.Permission>()
                .Where(p => allPermissions.HasFlag(p) && p != Quanlicuahang.Enum.Permission.None)
                .Select(p => p.ToString())
                .ToList();

            foreach (var perm in permissionNames)
            {
                claims.Add(new Claim("Permission", perm));
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