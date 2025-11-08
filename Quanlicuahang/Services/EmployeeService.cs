using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Employee;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System;

namespace Quanlicuahang.Services
{
    public interface IEmployeeService
    {
        Task<object> GetAllAsync(EmployeeSearchDto searchDto);
        Task<EmployeeDto?> GetByIdAsync(string id);
        Task<EmployeeDto> CreateAsync(EmployeeCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, EmployeeCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly IEmployeeRepository _repository;

        public EmployeeService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            IEmployeeRepository repository
        )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repository = repository;
        }

        // 🔹 Hàm sinh mã nhân viên tự động: EMP001, EMP002, ...
        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var lastEmployee = await _repository.GetAll(true)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastEmployee) && lastEmployee.StartsWith("EMP"))
            {
                if (int.TryParse(lastEmployee.Substring(3), out var current))
                {
                    nextNumber = current + 1;
                }
            }

            return $"EMP{nextNumber:D3}";
        }

        public async Task<EmployeeDto> CreateAsync(EmployeeCreateUpdateDto dto)
        {
            if (await _repository.ExistsAsync(e => e.Email == dto.Email))
                throw new System.Exception($"Email nhân viên '{dto.Email}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            string code = string.IsNullOrWhiteSpace(dto.Code)
                ? await GenerateEmployeeCodeAsync()
                : dto.Code.Trim();

            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                Code = code,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(employee);
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Employees",
                entityId: employee.Id,
                description: $"Tạo mới nhân viên {employee.Name}",
                oldValue: null,
                newValue: employee,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(employee.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, EmployeeCreateUpdateDto dto)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                throw new System.Exception("Nhân viên không tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { employee.Code, employee.Name, employee.Phone, employee.Email, employee.Address };

            employee.Code = string.IsNullOrWhiteSpace(dto.Code) ? employee.Code : dto.Code.Trim();
            employee.Name = dto.Name;
            employee.Phone = dto.Phone;
            employee.Email = dto.Email;
            employee.Address = dto.Address;
            employee.UpdatedBy = userId;
            employee.UpdatedAt = DateTime.UtcNow;

            _repository.Update(employee);
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Employees",
                entityId: employee.Id,
                description: $"Cập nhật nhân viên {employee.Name}",
                oldValue: oldValue,
                newValue: employee,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { employee.IsDeleted, employee.UpdatedAt };

            employee.IsDeleted = true;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Employees",
                entityId: employee.Id,
                description: $"Ngưng hoạt động nhân viên {employee.Name}",
                oldValue: oldValue,
                newValue: new { employee.IsDeleted, employee.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { employee.IsDeleted, employee.UpdatedAt };

            employee.IsDeleted = false;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Employees",
                entityId: employee.Id,
                description: $"Kích hoạt nhân viên {employee.Name}",
                oldValue: oldValue,
                newValue: new { employee.IsDeleted, employee.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetAllAsync(EmployeeSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _repository.GetAll(true);
            var where = searchDto.Where;

            if (where != null)
            {
                if (!string.IsNullOrWhiteSpace(where.Code))
                {
                    var code = where.Code.Trim().ToLower();
                    query = query.Where(e => e.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(e => e.Name.ToLower().Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(where.Phone))
                {
                    var phone = where.Phone.Trim().ToLower();
                    query = query.Where(e => e.Phone != null && e.Phone.ToLower().Contains(phone));
                }

                if (!string.IsNullOrWhiteSpace(where.Email))
                {
                    var email = where.Email.Trim().ToLower();
                    query = query.Where(e => e.Email != null && e.Email.ToLower().Contains(email));
                }

                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(e => e.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    Name = e.Name,
                    Phone = e.Phone,
                    Email = e.Email,
                    Address = e.Address,
                    IsDeleted = e.IsDeleted,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    CreatedBy = e.CreatedBy,
                    UpdatedBy = e.UpdatedBy,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !e.IsDeleted,
                    isCanDeActive = !e.IsDeleted,
                    isCanActive = e.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<EmployeeDto?> GetByIdAsync(string id)
        {
            return await _repository.GetAll(true)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    Name = e.Name,
                    Phone = e.Phone,
                    Email = e.Email,
                    Address = e.Address,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    IsDeleted = e.IsDeleted
                })
                .FirstOrDefaultAsync();
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repository.GetAll(false)
                .OrderBy(e => e.Name)
                .Select(e => new SelectBoxDto
                {
                    Id = e.Id,
                    Name = $"{e.Code} - {e.Name}"
                });

            var data = await query.ToListAsync();
            return new { data, total = data.Count };
        }
    }
}
