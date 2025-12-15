using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Category;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface ICategoryService
    {
        Task<object> GetAllAsync(CategorySearchDto searchDto);
        Task<CategoryDto?> GetByIdAsync(string id);
        Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, CategoryCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();
    }

    public class CategoryService : ICategoryService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly ICategoryRepository _repo;


        public CategoryService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
             ITokenHelper tokenHelper,
            ICategoryRepository repository
           )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repo = repository;

        }

        public async Task<object> GetAllAsync(CategorySearchDto searchDto)
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
                    query = query.Where(c => c.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(where.Description))
                {
                    var description = where.Description.Trim().ToLower();
                    query = query.Where(c => c.Description != null && c.Description.ToLower().Contains(description));
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
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
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

        public async Task<CategoryDto?> GetByIdAsync(string id)
        {
            var category = await _repo.GetAll(true)
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                })
                .FirstOrDefaultAsync();

            return category;
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(c => c.Code == dto.Code))
                throw new System.Exception($"Mã danh mục '{dto.Code}' đã tồn tại!");

            if (await _repo.ExistsAsync(c => c.Name == dto.Name))
                throw new System.Exception($"Tên danh mục '{dto.Name}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Categories",
                entityId: category.Id,
                description: $"Tạo mới danh mục {category.Code} - {category.Name}",
                oldValue: null,
                newValue: category,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(category.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, CategoryCreateUpdateDto dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) throw new System.Exception("Danh mục không tồn tại!");

            if (category.Code != dto.Code)
            {
                var codeExists = await _repo.ExistsAsync(
                    c => c.Code == dto.Code && !c.IsDeleted,
                    excludeId: id
                );

                if (codeExists)
                    throw new System.Exception($"Mã danh mục '{dto.Code}' đã tồn tại!");
            }

            if (category.Name != dto.Name)
            {
                var nameExists = await _repo.ExistsAsync(
                    c => c.Name == dto.Name && !c.IsDeleted,
                    excludeId: id
                );

                if (nameExists)
                    throw new System.Exception($"Tên danh mục '{dto.Name}' đã tồn tại!");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { category.Code, category.Name, category.Description, category.ImageUrl };
            category.Code = dto.Code;
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.UpdatedBy = userId;
            category.UpdatedAt = DateTime.UtcNow;
            _repo.Update(category);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Categories",
                entityId: category.Id,
                description: $"Cập nhật danh mục {category.Code} - {category.Name}",
                oldValue: oldValue,
                newValue: category,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                category.IsDeleted,
                category.UpdatedAt
            };

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Categories",
                entityId: category.Id,
                description: $"Ngưng hoạt động danh mục {category.Code} - {category.Name}",
                oldValue: oldValue,
                newValue: new { category.IsDeleted, category.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                category.IsDeleted,
                category.UpdatedAt
            };

            category.IsDeleted = false;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Categories",
                entityId: category.Id,
                description: $"Kích hoạt danh mục {category.Code} - {category.Name}",
                oldValue: oldValue,
                newValue: new { category.IsDeleted, category.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repo.GetAll(false)
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


    }
}