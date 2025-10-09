using Microsoft.EntityFrameworkCore;
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

        public async Task<object> GetCategoriesAsync(CategorySearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            // Lấy tất cả dữ liệu (bao gồm cả deleted nếu có điều kiện where.IsDeleted)
            var query = _repository.GetAll(true);

            // Áp dụng điều kiện where nếu có
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
            else
            {
                var results = query.ToList();

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
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            return new
            {
                skip,
                take,
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