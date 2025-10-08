using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Category;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface ICategoryService
    {
        Task<object> GetCategoriesAsync(string? search, int page, int pageSize, bool includeDeleted);
        Task<CategoryDto?> GetCategoryByIdAsync(string id);
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto);
        Task<bool> UpdateCategoryAsync(string id, CategoryCreateUpdateDto dto);
        Task<bool> SoftDeleteCategoryAsync(string id);
        Task<bool> RestoreCategoryAsync(string id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> GetCategoriesAsync(string? search, int page, int pageSize, bool includeDeleted)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _repository.GetAll(includeDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Code.ToLower().Contains(search));
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync();

            return new
            {
                totalRecords,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                items
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
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();

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
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            category.Code = dto.Code;
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(category);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteCategoryAsync(string id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreCategoryAsync(string id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return false;

            category.IsDeleted = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
