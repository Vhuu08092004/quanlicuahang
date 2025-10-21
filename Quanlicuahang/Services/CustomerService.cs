using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Customer;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface ICustomerService
    {
        Task<object> GetAllAsync(CustomerSearchDto searchDto);
        Task<CustomerDto?> GetByIdAsync(string id);
        Task<CustomerDto> CreateAsync(CustomerCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, CustomerCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();
    }

    public class CustomerService : ICustomerService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;

        private readonly ITokenHelper _tokenHelper;
        private readonly ICustomerRepository _repository;

        public CustomerService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            ICustomerRepository repository
        )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repository = repository;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                customer.IsDeleted,
                customer.UpdatedAt
            };

            customer.IsDeleted = false;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = userId;
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Customers",
                entityId: customer.Id,
                description: $"Kích hoạt khách hàng {customer.Code} - {customer.Name}",
                oldValue: oldValue,
                newValue: new { customer.IsDeleted, customer.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateUpdateDto dto)
        {
            if (await _repository.ExistsAsync(c => c.Code == dto.Code))
                throw new System.Exception($"Mã khách hàng '{dto.Code}' đã tồn tại!");

            if (await _repository.ExistsAsync(c => c.Email == dto.Email))
                throw new System.Exception($"Email khách hàng '{dto.Email}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var customer = new Customer
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

            await _repository.AddAsync(customer);
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Customers",
                entityId: customer.Id,
                description: $"Tạo mới khách hàng {customer.Code} - {customer.Name}",
                oldValue: null,
                newValue: customer,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(customer.Id))!;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                customer.IsDeleted,
                customer.UpdatedAt
            };

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = userId;
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Customers",
                entityId: customer.Id,
                description: $"Ngưng hoạt động khách hàng {customer.Code} - {customer.Name}",
                oldValue: oldValue,
                newValue: new { customer.IsDeleted, customer.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetAllAsync(CustomerSearchDto searchDto)
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
                    query = query.Where(c => c.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(where.Phone))
                {
                    var phone = where.Phone.Trim().ToLower();
                    query = query.Where(c => c.Phone != null && c.Phone.ToLower().Contains(phone));
                }

                if (!string.IsNullOrWhiteSpace(where.Email))
                {
                    var email = where.Email.Trim().ToLower();
                    query = query.Where(c => c.Email != null && c.Email.ToLower().Contains(email));
                }

                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(c => c.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    IsDeleted = c.IsDeleted,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedBy = c.UpdatedBy,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !c.IsDeleted,
                    isCanDeActive = !c.IsDeleted,
                    isCanActive = c.IsDeleted
                })
                .ToListAsync();

            return new
            {
                data,
                total
            };
        }

        public async Task<CustomerDto?> GetByIdAsync(string id)
        {
            var customer = await _repository.GetAll(true)
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                })
                .FirstOrDefaultAsync();

            return customer;
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repository.GetAll(false)
                .OrderBy(c => c.Name)
                .Select(c => new SelectBoxDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name
                });

            var data = await query.ToListAsync();
            var total = data.Count;

            return new
            {
                data,
                total
            };
        }

        public async Task<bool> UpdateAsync(string id, CustomerCreateUpdateDto dto)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) throw new System.Exception("Khách hàng không tồn tại!");

            if (customer.Code != dto.Code)
            {
                var codeExists = await _repository.ExistsAsync(
                    c => c.Code == dto.Code && !c.IsDeleted,
                    excludeId: id
                );

                if (codeExists)
                    throw new System.Exception($"Mã khách hàng '{dto.Code}' đã tồn tại!");
            }

            if (customer.Name != dto.Name)
            {
                var nameExists = await _repository.ExistsAsync(
                    c => c.Name == dto.Name && !c.IsDeleted,
                    excludeId: id
                );

                if (nameExists)
                    throw new System.Exception($"Tên khách hàng '{dto.Name}' đã tồn tại!");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { customer.Code, customer.Name, customer.Phone, customer.Email, customer.Address };
            customer.Code = dto.Code;
            customer.Name = dto.Name;
            customer.Phone = dto.Phone;
            customer.Email = dto.Email;
            customer.Address = dto.Address;
            customer.UpdatedBy = userId;
            customer.UpdatedAt = DateTime.UtcNow;
            _repository.Update(customer);
            await _repository.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Customers",
                entityId: customer.Id,
                description: $"Cập nhật khách hàng {customer.Code} - {customer.Name}",
                oldValue: oldValue,
                newValue: customer,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }
    }
}
