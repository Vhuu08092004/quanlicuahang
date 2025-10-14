using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Quanlicuahang.Services
{
    public interface IUserService
    {
        Task<object> GetAllAsync(UserSearchDto searchDto);
        Task<UserDto?> GetByIdAsync(string id);
        Task<UserDto> CreateAsync(UserCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, UserCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IActionLogService _logService;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;


        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }


        public UserService(IUserRepository userRepo, IRoleRepository roleRepo, IUserRoleRepository userRoleRepo, IActionLogService logService, IHttpContextAccessor httpContext, ITokenHelper tokenHelper)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(UserSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _userRepo.GetAll()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchDto.Username))
                query = query.Where(u => u.Username.Contains(searchDto.Username.Trim()));

            if (!string.IsNullOrWhiteSpace(searchDto.FullName))
                query = query.Where(u => u.FullName != null && u.FullName.Contains(searchDto.FullName.Trim()));

            if (searchDto.IsDeleted.HasValue)
                query = query.Where(u => u.IsDeleted == searchDto.IsDeleted.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt,
                    CreatedBy = u.CreatedBy,
                    UpdatedAt = u.UpdatedAt,
                    UpdatedBy = u.UpdatedBy,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !u.IsDeleted,
                    isCanDeActive = !u.IsDeleted,
                    isCanActive = u.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userRepo.GetAll()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                isCanView = true,
                isCanCreate = true,
                isCanEdit = !user.IsDeleted,
                isCanDeActive = !user.IsDeleted,
                isCanActive = user.IsDeleted
            };
        }

        public async Task<UserDto> CreateAsync(UserCreateUpdateDto dto)
        {
            if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
                throw new System.Exception($"Username '{dto.Username}' đã tồn tại!");
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = dto.Username,
                Password = HashPassword(dto.Password ?? "defaultPassword123"),
                FullName = dto.FullName,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                EmployeeId = dto.EmployeeId
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                foreach (var roleId in dto.RoleIds)
                {
                    await _userRoleRepo.AddUserRoleAsync(user.Id, roleId);

                }
                await _userRepo.SaveChangesAsync();
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "User",
                entityId: user.Id,
                description: $"Tạo mới người dùng {user.Username}",
                oldValue: null,
                newValue: user,
                userId: userId,
               ip: ip,
                userAgent: agent
            );

            return await GetByIdAsync(user.Id) ?? throw new System.Exception("Tạo người dùng thất bại!");
        }

        public async Task<bool> UpdateAsync(string id, UserCreateUpdateDto dto)
        {
            var user = await _userRepo.GetAll()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) throw new System.Exception("Người dùng không tồn tại!");

            if (!string.Equals(user.Username, dto.Username, StringComparison.OrdinalIgnoreCase) &&
                await _userRepo.GetByUsernameAsync(dto.Username) != null)
            {
                throw new System.Exception($"Username '{dto.Username}' đã tồn tại!");
            }

            var oldValue = new { user.Username, user.FullName, Roles = user.UserRoles.Select(ur => ur.RoleId).ToList() };
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            user.Username = dto.Username;
            user.FullName = dto.FullName;
            user.EmployeeId = dto.EmployeeId;
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Password))
                user.Password = HashPassword(dto.Password);

            await _userRoleRepo.RemoveAllRolesByUserIdAsync(user.Id);

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                foreach (var roleId in dto.RoleIds)
                {
                    await _userRoleRepo.AddUserRoleAsync(user.Id, roleId);
                }
            }


            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Users",
                entityId: user.Id,
                description: $"Cập nhật người dùng {user.Username}",
                oldValue: oldValue,
                newValue: user,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            var oldValue = new { user.IsDeleted, user.UpdatedAt };
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            user.IsDeleted = true;
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "User",
                entityId: user.Id,
                description: $"Ngưng hoạt động người dùng {user.Username}",
                oldValue: oldValue,
                newValue: new { user.IsDeleted, user.UpdatedAt },
                userId: userId,
               ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            var oldValue = new { user.IsDeleted, user.UpdatedAt };

            user.IsDeleted = false;
            user.UpdatedBy = userId;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "User",
                entityId: user.Id,
                description: $"Kích hoạt người dùng {user.Username}",
                oldValue: oldValue,
                newValue: new { user.IsDeleted, user.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }
    }
}
