using Microsoft.EntityFrameworkCore;
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
        Task<ProductAttributeDto> UpdateAsync(string id, ProductAttributeCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IProductAttributeRepository _repo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public ProductAttributeService(
            ITokenHelper tokenHelper,
            IProductAttributeRepository repo, IActionLogService logService, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(ProductAttributeSearchDto searchDto)
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
                .OrderByDescending(pa => pa.CreatedAt)
                .Skip(searchDto.Skip)
                .Take(searchDto.Take)
                .Select(pa => new ProductAttributeDto
                {
                    Id = pa.Id,
                    Code = pa.Code,
                    Name = pa.Name,
                    Description = pa.Description,
                    IsDeleted = pa.IsDeleted,
                    CreatedAt = pa.CreatedAt,
                    UpdatedAt = pa.UpdatedAt,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !pa.IsDeleted,
                    isCanDeActive = !pa.IsDeleted,
                    isCanActive = pa.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductAttributeDto?> GetByIdAsync(string id)
        {
            var attribute = await _repo.GetAll(true)
                .Include(a => a.AttributeValues)
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
            if (attribute == null) return null;
            var productAttribute = new ProductAttributeDto
            {
                Id = attribute.Id,
                Code = attribute.Code,
                Name = attribute.Name,
                Description = attribute.Description,
                AttributeValues = attribute.AttributeValues
                    .Where(av => !av.IsDeleted)
                    .OrderBy(av => av.DisplayOrder)
                    .Select(av => new ProductAttributeValueDto
                    {
                        Id = av.Id,
                        AttributeId = av.AttributeId,
                        AttributeName = attribute.Name,
                        Value = av.Value,
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

            return productAttribute;
        }


        public async Task<ProductAttributeDto> CreateAsync(ProductAttributeCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(p => p.Code == dto.Code))
                throw new System.Exception($"Mã thuộc tính sản phẩm'{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var productattr = new ProductAttribute
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(productattr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "ProductAttributes",
                entityId: productattr.Id,
                description: $"Tạo mới thuộc tính sản phẩm {productattr.Code} - {productattr.Name}",
                oldValue: null,
                newValue: productattr,
                userId: userId,
                 ip: ip,
                userAgent: agent);

            return (await GetByIdAsync(productattr.Id))!;
        }

        public async Task<ProductAttributeDto> UpdateAsync(string id, ProductAttributeCreateUpdateDto dto)
        {
            var productattr = await _repo.GetByIdAsync(id);
            if (productattr == null)
                throw new System.Exception("Thuộc tính sản phẩm không tồn tại");

            if (await _repo.ExistsAsync(p => p.Code == dto.Code, id))
                throw new System.Exception($"Mã thuộc tính '{dto.Code}' đã tồn tại");

            var oldValue = new { productattr.Code, productattr.Name };
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            productattr.Code = dto.Code;
            productattr.Name = dto.Name;
            productattr.Description = dto.Description;
            productattr.UpdatedBy = userId;
            productattr.UpdatedAt = DateTime.UtcNow;

            _repo.Update(productattr);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
               code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "ProductAttributes",
                entityId: productattr.Id,
                description: $"Cập nhật thuộc tính sản phẩm {productattr.Code} - {productattr.Name}",
                oldValue: oldValue,
                newValue: productattr,
                userId: userId,
                 ip: ip,
                userAgent: agent
                );

            return (await GetByIdAsync(productattr.Id))!;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var productattr = await _repo.GetByIdAsync(id);
            if (productattr == null) return false;

            var oldValue = new
            {
                productattr.IsDeleted,
                productattr.UpdatedAt
            };

            productattr.IsDeleted = true;
            productattr.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "ProductAttributes",
                entityId: productattr.Id,
                description: $"Ngưng hoạt động thuộc tính sản phẩm {productattr.Code} - {productattr.Name}",
                oldValue: oldValue,
                newValue: new { productattr.IsDeleted, productattr.UpdatedAt },
                userId: userId,
                 ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var productattr = await _repo.GetByIdAsync(id);
            if (productattr == null) return false;

            var oldValue = new
            {
                productattr.IsDeleted,
                productattr.UpdatedAt
            };

            productattr.IsDeleted = false;
            productattr.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();


            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "ProductAttributes",
                entityId: productattr.Id,
                description: $"Kích hoạt thuộc tính sản phẩm {productattr.Code} - {productattr.Name}",
                oldValue: oldValue,
                newValue: new { productattr.IsDeleted, productattr.UpdatedAt },
                userId: userId,
                 ip: ip,
                userAgent: agent
            );

            return true;
        }
    }
}