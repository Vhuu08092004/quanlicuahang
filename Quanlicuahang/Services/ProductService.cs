using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Product;
using Quanlicuahang.DTOs.ProductVariant;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IProductService
    {
        Task<object> GetAllAsync(ProductSearchDto searchDto);
        Task<ProductDto?> GetByIdAsync(string id);
        Task<ProductDto> CreateAsync(ProductCreateUpdateDto dto);
        Task<ProductDto> UpdateAsync(string id, ProductCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;


        public ProductService(
            IProductRepository repo,
            IProductVariantRepository variantRepo,
            IProductVariantAttributeValueRepository variantAttrRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _repo = repo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(ProductSearchDto searchDto)
        {

            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _repo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

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
                if (!string.IsNullOrWhiteSpace(where.CategoryId))
                    query = query.Where(p => p.CategoryId == where.CategoryId);
                if (!string.IsNullOrWhiteSpace(where.SupplierId))
                    query = query.Where(p => p.SupplierId == where.SupplierId);
                if (where.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= where.MinPrice.Value);
                if (where.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= where.MaxPrice.Value);
            }

            var total = await query.CountAsync();

            var data = await query
                 .OrderByDescending(c => c.CreatedAt)
                .Skip(searchDto.Skip)
                .Take(searchDto.Take)
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
                    UpdatedBy = p.UpdatedBy,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductDto?> GetByIdAsync(string id)
        {
            var product = await _repo.GetAll(true)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantValues)
                        .ThenInclude(vv => vv.AttributeValue)
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
                    Variants = p.Variants.Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        Code = v.Code,
                        Name = v.Name,
                        SKU = v.SKU,
                        PriceAdjustment = v.PriceAdjustment,
                        StockQuantity = v.StockQuantity,
                        ProductId = v.ProductId,
                        ProductName = p.Name,
                        Attributes = v.VariantValues.Select(vv => new VariantAttributeDto
                        {
                            AttributeName = vv.AttributeValue.Attribute.Name,
                            AttributeValue = vv.AttributeValue.Value,
                            AttributeValueId = vv.AttributeValueId
                        }).ToList(),
                        IsDeleted = v.IsDeleted,
                        CreatedAt = v.CreatedAt,
                        UpdatedAt = v.UpdatedAt
                    }).ToList(),
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    UpdatedBy = p.UpdatedBy,
                    CreatedBy = p.CreatedBy
                })
                .FirstOrDefaultAsync();

            return product;
        }


        public async Task<ProductDto> CreateAsync(ProductCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(p => p.Code == dto.Code))
                throw new System.Exception($"Mã sản phẩm'{dto.Code}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
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
                CreatedBy = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            await _repo.SaveChangesAsync();


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

        public async Task<ProductDto> UpdateAsync(string id, ProductCreateUpdateDto dto)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                throw new System.Exception("Không tìm thấy sản phẩm");

            if (await _repo.ExistsAsync(p => p.Code == dto.Code, id))
                throw new System.Exception($"Mã sản phẩm '{dto.Code}' đã tồn tại");

            var oldValue = new { product.Code, product.Name, product.Price };

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            product.Code = dto.Code;
            product.Name = dto.Name;
            product.Barcode = dto.Barcode;
            product.Price = dto.Price;
            product.Unit = dto.Unit;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
            product.UpdatedBy = userId ?? string.Empty;
            product.UpdatedAt = DateTime.UtcNow;

            _repo.Update(product);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Products",
                entityId: product.Id,
                description: $"Cập nhật sản phẩm {product.Code} - {product.Name}",
                oldValue: oldValue,
                newValue: product,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(product.Id))!;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            var oldValue = new
            {
                product.IsDeleted,
                product.UpdatedAt
            };

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
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

            var oldValue = new
            {
                product.IsDeleted,
                product.UpdatedAt
            };

            product.IsDeleted = false;
            product.UpdatedAt = DateTime.UtcNow;
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
    }
}