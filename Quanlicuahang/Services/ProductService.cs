using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Product;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IProductService
    {
        Task<object> GetAllAsync(ProductSearchDto searchDto);
        Task<ProductDto?> GetByIdAsync(string id);
        Task<ProductDto> CreateAsync(ProductCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, ProductCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetSelectBoxAsync();
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IProductAttributeRepository _attributeRepo;
        private readonly IProductAttributeValueRepository _attributeValueRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public ProductService(
            IProductRepository repo,
            IProductAttributeRepository attributeRepo,
            IProductAttributeValueRepository attributeValueRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _repo = repo;
            _attributeRepo = attributeRepo;
            _attributeValueRepo = attributeValueRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(ProductSearchDto searchDto)
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
                    query = query.Where(p => p.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(where.CategoryId))
                {
                    query = query.Where(p => p.CategoryId != null && p.CategoryId.ToString() == where.CategoryId.Trim());
                }

                if (!string.IsNullOrWhiteSpace(where.SupplierId))
                {
                    query = query.Where(p => p.SupplierId != null && p.SupplierId.ToString() == where.SupplierId.Trim());
                }

                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(p => p.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Unit = p.Unit,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !p.IsDeleted,
                    isCanDeActive = !p.IsDeleted,
                    isCanActive = p.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductDto?> GetByIdAsync(string id)
        {
            var product = await _repo.GetAll(true)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Unit = p.Unit,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                    Attributes = p.AttributeValues
                        .Where(av => !av.IsDeleted)
                        .OrderBy(av => av.DisplayOrder)
                        .Select(av => new ProductAttributeValueDto
                        {
                            Id = av.Id,
                            AttributeId = av.AttributeId,
                            AttributeName = av.Attribute.Name,
                            AttributeCode = av.Attribute.Code,
                            ValueString = av.ValueString,
                            ValueDecimal = av.ValueDecimal,
                            ValueInt = av.ValueInt,
                            ValueBool = av.ValueBool,
                            ValueDate = av.ValueDate,
                            DisplayOrder = av.DisplayOrder,
                            IsDeleted = av.IsDeleted,
                            CreatedAt = av.CreatedAt,
                            UpdatedAt = av.UpdatedAt
                        }).ToList(),
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return product;
        }

        public async Task<ProductDto> CreateAsync(ProductCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(p => p.Code == dto.Code))
                throw new System.Exception($"Mã sản phẩm '{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                Barcode = dto.Barcode,
                Price = dto.Price,
                Unit = dto.Unit,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            await _repo.SaveChangesAsync();

            // Log creation
            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Products",
                entityId: product.Id,
                description: $"Tạo mới sản phẩm {product.Code} - {product.Name}",
                oldValue: null,
                newValue: product,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(product.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, ProductCreateUpdateDto dto)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) throw new System.Exception("Sản phẩm không tồn tại!");

            if (product.Code != dto.Code && await _repo.ExistsAsync(p => p.Code == dto.Code && !p.IsDeleted, id))
                throw new System.Exception($"Mã sản phẩm '{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                product.Code,
                product.Name,
                product.Barcode,
                product.Price,
                product.Unit,
                product.CategoryId,
                product.SupplierId
            };

            product.Code = dto.Code;
            product.Name = dto.Name;
            product.Barcode = dto.Barcode;
            product.Price = dto.Price;
            product.Unit = dto.Unit;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;

            _repo.Update(product);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Products",
                entityId: product.Id,
                description: $"Cập nhật sản phẩm {product.Code} - {product.Name}",
                oldValue: oldValue,
                newValue: product,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { product.IsDeleted, product.UpdatedAt };

            product.IsDeleted = true;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;
            _repo.Update(product);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Products",
                entityId: product.Id,
                description: $"Ngưng hoạt động sản phẩm {product.Code} - {product.Name}",
                oldValue: oldValue,
                newValue: new { product.IsDeleted, product.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { product.IsDeleted, product.UpdatedAt };

            product.IsDeleted = false;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;
            _repo.Update(product);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Products",
                entityId: product.Id,
                description: $"Kích hoạt sản phẩm {product.Code} - {product.Name}",
                oldValue: oldValue,
                newValue: new { product.IsDeleted, product.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repo.GetAll(false)
                .OrderBy(p => p.Name)
                .Select(p => new SelectBoxDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name
                });

            var data = await query.ToListAsync();
            var total = data.Count;

            return new { data, total };
        }
    }
}
