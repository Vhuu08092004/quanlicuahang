using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IProductAttributeService
    {
        Task<object> GetAllAsync(ProductAttributeSearchDto searchDto);
        Task<ProductAttributeDto?> GetByIdAsync(string id);
        Task<ProductAttributeDto> CreateAsync(ProductAttributeCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, ProductAttributeCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();
    }

    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IProductAttributeRepository _repo;
        private readonly IProductAttributeValueRepository _attributeValueRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public ProductAttributeService(
            IProductAttributeRepository repo,
            IProductAttributeValueRepository attributeValueRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _repo = repo;
            _attributeValueRepo = attributeValueRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(ProductAttributeSearchDto searchDto)
        {
            var query = _repo.GetAll(true);

            if (searchDto.Where != null)
            {
                var where = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(where.Code))
                    query = query.Where(a => a.Code.ToLower().Contains(where.Code.Trim().ToLower()));
                if (!string.IsNullOrWhiteSpace(where.Name))
                    query = query.Where(a => a.Name.ToLower().Contains(where.Name.Trim().ToLower()));
                if (!string.IsNullOrWhiteSpace(where.Description))
                    query = query.Where(a => a.Description != null && a.Description.ToLower().Contains(where.Description.Trim().ToLower()));
                if (where.IsDeleted.HasValue)
                    query = query.Where(a => a.IsDeleted == where.IsDeleted.Value);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ProductAttributeDto
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Description = a.Description,
                    DataType = a.DataType,
                    IsDeleted = a.IsDeleted,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductAttributeDto?> GetByIdAsync(string id)
        {
            var attribute = await _repo.GetAll(true)
                .Include(a => a.AttributeValues)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null) return null;

            return new ProductAttributeDto
            {
                Id = attribute.Id,
                Code = attribute.Code,
                Name = attribute.Name,
                Description = attribute.Description,
                DataType = attribute.DataType,
                AttributeValues = attribute.AttributeValues
                    .Where(av => !av.IsDeleted)
                    .OrderBy(av => av.DisplayOrder)
                    .Select(av => new ProductAttributeValueDto
                    {
                        Id = av.Id,
                        AttributeId = av.AttributeId,
                        AttributeName = attribute.Name,
                        AttributeCode = attribute.Code,
                        ValueString = av.ValueString,
                        ValueDecimal = av.ValueDecimal,
                        ValueInt = av.ValueInt,
                        ValueBool = av.ValueBool,
                        ValueDate = av.ValueDate,
                        DisplayOrder = av.DisplayOrder,
                        IsDeleted = av.IsDeleted,
                        CreatedAt = av.CreatedAt,
                        UpdatedAt = av.UpdatedAt
                    })
                    .ToList(),
                IsDeleted = attribute.IsDeleted,
                CreatedAt = attribute.CreatedAt,
                UpdatedAt = attribute.UpdatedAt
            };
        }

        public async Task<ProductAttributeDto> CreateAsync(ProductAttributeCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(a => a.Code == dto.Code))
                throw new UnauthorizedAccessException($"Mã thuộc tính '{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng.");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var attr = new ProductAttribute
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                DataType = dto.DataType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await _repo.AddAsync(attr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "ProductAttributes",
                entityId: attr.Id,
                description: $"Tạo mới thuộc tính {attr.Name}",
                oldValue: null,
                newValue: attr,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return await GetByIdAsync(attr.Id) ?? throw new UnauthorizedAccessException("Tạo thuộc tính thất bại");
        }

        public async Task<bool> UpdateAsync(string id, ProductAttributeCreateUpdateDto dto)
        {
            var attr = await _repo.GetByIdAsync(id) ?? throw new UnauthorizedAccessException("Thuộc tính không tồn tại");

            if (await _repo.ExistsAsync(a => a.Code == dto.Code, id))
                throw new UnauthorizedAccessException($"Mã thuộc tính '{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng.");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                attr.Code,
                attr.Name,
                attr.Description,
                attr.DataType
            };

            attr.Code = dto.Code;
            attr.Name = dto.Name;
            attr.Description = dto.Description;
            attr.DataType = dto.DataType;
            attr.UpdatedAt = DateTime.UtcNow;
            attr.UpdatedBy = userId;

            _repo.Update(attr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "ProductAttributes",
                entityId: attr.Id,
                description: $"Cập nhật thuộc tính {attr.Name}",
                oldValue: oldValue,
                newValue: attr,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var attr = await _repo.GetByIdAsync(id);
            if (attr == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { attr.IsDeleted, attr.UpdatedAt };

            attr.IsDeleted = true;
            attr.UpdatedAt = DateTime.UtcNow;
            attr.UpdatedBy = userId;

            _repo.Update(attr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "ProductAttributes",
                entityId: attr.Id,
                description: $"Ngưng hoạt động thuộc tính {attr.Name}",
                oldValue: oldValue,
                newValue: new { attr.IsDeleted, attr.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var attr = await _repo.GetByIdAsync(id);
            if (attr == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { attr.IsDeleted, attr.UpdatedAt };

            attr.IsDeleted = false;
            attr.UpdatedAt = DateTime.UtcNow;
            attr.UpdatedBy = userId;

            _repo.Update(attr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "ProductAttributes",
                entityId: attr.Id,
                description: $"Kích hoạt thuộc tính {attr.Name}",
                oldValue: oldValue,
                newValue: new { attr.IsDeleted, attr.UpdatedAt },
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

            return new { data, total };
        }
    }
}
