using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Supplier;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface ISupplierService
    {
        Task<object> GetAllAsync(SupplierSearchDto searchDto);
        Task<SupplierDto?> GetByIdAsync(string id);
        Task<SupplierDto> CreateAsync(SupplierCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, SupplierCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();

    }

    public class SupplierService : ISupplierService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ISupplierRepository _repo;
        private readonly ITokenHelper _tokenHelper;

        public SupplierService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ISupplierRepository repository,
            ITokenHelper tokenHelper)
        {
            _logService = logService;
            _httpContext = httpContext;
            _repo = repository;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(SupplierSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _repo.GetAll(true);

            if (searchDto.Where != null)
            {
                var where = searchDto.Where;

                if (!string.IsNullOrWhiteSpace(where.Code))
                {
                    var code = where.Code.Trim().ToLower();
                    query = query.Where(s => s.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(where.Phone))
                {
                    var phone = where.Phone.Trim().ToLower();
                    query = query.Where(s => s.Phone != null && s.Phone.ToLower().Contains(phone));
                }

                if (!string.IsNullOrWhiteSpace(where.Address))
                {
                    var address = where.Address.Trim().ToLower();
                    query = query.Where(s => s.Address != null && s.Address.ToLower().Contains(address));
                }

                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(s => s.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    IsDeleted = s.IsDeleted,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !s.IsDeleted,
                    isCanDeActive = !s.IsDeleted,
                    isCanActive = s.IsDeleted
                })
                .ToListAsync();

            return new
            {
                data,
                total
            };
        }

        public async Task<SupplierDto?> GetByIdAsync(string id)
        {
            var supplier = await _repo.GetAll(true)
                .Where(s => s.Id == id)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Phone = s.Phone,
                    Address = s.Address,
                    Email = s.Email,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    IsDeleted = s.IsDeleted
                })
                .FirstOrDefaultAsync();

            return supplier;
        }

        public async Task<SupplierDto> CreateAsync(SupplierCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(s => s.Code == dto.Code))
                throw new System.Exception($"Mã nhà cung cấp '{dto.Code}' đã tồn tại!");

            if (await _repo.ExistsAsync(s => s.Name == dto.Name))
                throw new System.Exception($"Tên nhà cung cấp '{dto.Name}' đã tồn tại!");

            if (await _repo.ExistsAsync(s => s.Email == dto.Email))
                throw new System.Exception($"Email nhà cung cấp '{dto.Email}' đã tồn tại!");


            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var supplier = new Supplier
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(supplier);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Suppliers",
                entityId: supplier.Id,
                description: $"Tạo mới nhà cung cấp {supplier.Code} - {supplier.Name}",
                oldValue: null,
                newValue: supplier,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(supplier.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, SupplierCreateUpdateDto dto)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null) throw new System.Exception("nhà cung cấp không tồn tại!");

            if (supplier.Code != dto.Code)
            {
                var codeExists = await _repo.ExistsAsync(
                    s => s.Code == dto.Code && !s.IsDeleted,
                    excludeId: id
                );

                if (codeExists)
                    throw new System.Exception($"Mã nhà cung cấp '{dto.Code}' đã tồn tại!");
            }

            if (supplier.Name != dto.Name)
            {
                var nameExists = await _repo.ExistsAsync(
                    s => s.Name == dto.Name && !s.IsDeleted,
                    excludeId: id
                );

                if (nameExists)
                    throw new System.Exception($"Tên nhà cung cấp '{dto.Name}' đã tồn tại!");
            }

            if (supplier.Email != dto.Email)
            {
                var emailExists = await _repo.ExistsAsync(
                    s => s.Email == dto.Email && !s.IsDeleted,
                    excludeId: id
                );

                if (emailExists)
                    throw new System.Exception($"Email nhà cung cấp '{dto.Name}' đã tồn tại!");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { supplier.Code, supplier.Name, supplier.Phone, supplier.Address, supplier.UpdatedBy, supplier.UpdatedAt };
            supplier.Code = dto.Code;
            supplier.Name = dto.Name;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;
            supplier.UpdatedBy = userId;
            supplier.UpdatedAt = DateTime.UtcNow;
            _repo.Update(supplier);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Suppliers",
                entityId: supplier.Id,
                description: $"Cập nhật nhà cung cấp {supplier.Code} - {supplier.Name}",
                oldValue: oldValue,
                newValue: supplier,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                supplier.IsDeleted,
                supplier.UpdatedAt
            };

            supplier.IsDeleted = true;
            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Suppliers",
                entityId: supplier.Id,
                description: $"Ngưng hoạt động nhà cung cấp {supplier.Code} - {supplier.Name}",
                oldValue: oldValue,
                newValue: new { supplier.IsDeleted, supplier.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                supplier.IsDeleted,
                supplier.UpdatedAt
            };

            supplier.IsDeleted = false;
            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Suppliers",
                entityId: supplier.Id,
                description: $"Kích hoạt nhà cung cấp {supplier.Code} - {supplier.Name}",
                oldValue: oldValue,
                newValue: new { supplier.IsDeleted, supplier.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repo.GetAll(false)
                .OrderBy(s => s.Name)
                .Select(s => new SelectBoxDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                });

            var data = await query.ToListAsync();
            var total = data.Count;

            return new
            {
                data,
                total
            };
        }


    }
}