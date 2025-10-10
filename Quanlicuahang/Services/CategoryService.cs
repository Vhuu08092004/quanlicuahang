using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Category;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface ICategoryService
    {
        Task<object> GetCategoriesAsync(CategorySearchDto searchDto);
        Task<CategoryDto?> GetCategoryByIdAsync(string id);
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto);
        Task<bool> UpdateCategoryAsync(string id, CategoryCreateUpdateDto dto);
        Task<bool> DeActiveCategoryAsync(string id);
        Task<bool> ActiveCategoryAsync(string id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ICategoryRepository _repository;

        public CategoryService( ApplicationDbContext context, IActionLogService logService,  IHttpContextAccessor httpContext,ICategoryRepository repository)
        {
            _context = context;
            _logService = logService;
            _httpContext = httpContext;
            _repository = repository; 
        }


        public async Task<object> GetCategoriesAsync(CategorySearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            // Lấy tất cả dữ liệu (bao gồm cả deleted nếu có điều kiện where.IsDeleted)
            var query = _repository.GetAll(true);

            // Áp dụng điều kiện lọc nếu có
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
                .Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.Name,
                    c.Description,
                    c.IsDeleted,
                    c.CreatedAt,
                    c.UpdatedAt,
                    CanView = true,           // luôn có thể xem
                    CanCreate = true,         // luôn có thể tạo
                    CanEdit = !c.IsDeleted,   // chỉ edit nếu chưa xóa
                    CanDeActive = !c.IsDeleted, // chỉ deactive nếu chưa xóa
                    CanActive = c.IsDeleted     // chỉ active nếu đang xóa
                })
                .ToListAsync();

            return new
            {
                data,
                total
            };
        }


        public async Task<CategoryDto?> GetCategoryByIdAsync(string id)
        {
            var category = await _repository.GetAll(true)
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                })
                .FirstOrDefaultAsync();

            return category;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var userId = _httpContext.HttpContext?.User?.Identity?.Name;
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

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

            return new CategoryDto
            {
                Id = category.Id,
                Code = category.Code,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                IsDeleted = category.IsDeleted
            };
        }


        public async Task<bool> UpdateCategoryAsync(string id, CategoryCreateUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            var oldValue = new {category.Code, category.Name, category.Description };
            category.Code = dto.Code;
            category.Name = dto.Name;
            category.Description = dto.Description;
            await _context.SaveChangesAsync();

            var userId = _httpContext.HttpContext?.User?.Identity?.Name;
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

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

        public async Task<bool> DeActiveCategoryAsync(string id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            var oldValue = new
            {
                category.IsDeleted,
                category.UpdatedAt
            };

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();

            var userId = _httpContext.HttpContext?.User?.Identity?.Name;
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

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

        public async Task<bool> ActiveCategoryAsync(string id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            var oldValue = new
            {
                category.IsDeleted,
                category.UpdatedAt
            };

            category.IsDeleted = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();

            var userId = _httpContext.HttpContext?.User?.Identity?.Name;
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

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

    }
}