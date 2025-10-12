using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.ProductVariant;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IProductVariantService
    {
        Task<object> GetAllAsync(ProductVariantSearchDto searchDto);
        Task<ProductVariantDto?> GetByIdAsync(string id);
        Task<ProductVariantDto> CreateAsync(ProductVariantCreateUpdateDto dto);
        Task<ProductVariantDto> UpdateAsync(string id, ProductVariantCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly IProductVariantAttributeValueRepository _variantAttrRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;


        public ProductVariantService(
            IProductVariantRepository repo,
            IProductRepository productRepo,
            IProductVariantAttributeValueRepository variantAttrRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper
            )
        {
            _repo = repo;
            _productRepo = productRepo;
            _variantAttrRepo = variantAttrRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;

        }

        public async Task<object> GetAllAsync(ProductVariantSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;
            var query = _repo.GetAll()
                .Include(pv => pv.Product)
                .AsQueryable();

            if (searchDto.Where != null)
            {
                var w = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(w.Code))
                {
                    var code = w.Code.Trim().ToLower();
                    query = query.Where(c => c.Code.ToLower().Contains(code));
                }

                if (!string.IsNullOrWhiteSpace(w.Name))
                {
                    var name = w.Name.Trim().ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(w.ProductId))
                    query = query.Where(pv => pv.ProductId == w.ProductId);
                if (w.InStock.HasValue)
                {
                    if (w.InStock.Value)
                        query = query.Where(pv => pv.StockQuantity > 0);
                    else
                        query = query.Where(pv => pv.StockQuantity == 0);
                }
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(pv => pv.CreatedAt)
                .Skip(searchDto.Skip)
                .Take(searchDto.Take)
                .Select(pv => new ProductVariantDto
                {
                    Id = pv.Id,
                    Code = pv.Code,
                    ProductId = pv.ProductId,
                    ProductName = pv.Product.Name,
                    Name = pv.Name,
                    SKU = pv.SKU,
                    PriceAdjustment = pv.PriceAdjustment,
                    StockQuantity = pv.StockQuantity,
                    IsDeleted = pv.IsDeleted,
                    CreatedAt = pv.CreatedAt,
                    UpdatedAt = pv.UpdatedAt
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductVariantDto?> GetByIdAsync(string id)
        {
            var variant = await _repo.GetAll(true)
                .Include(v => v.Product)
                .Include(v => v.VariantValues)
                    .ThenInclude(vv => vv.AttributeValue)
                        .ThenInclude(av => av.Attribute)
                .Where(v => v.Id == id)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    Name = v.Name,
                    SKU = v.SKU,
                    PriceAdjustment = v.PriceAdjustment,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.VariantValues
                        .Where(vv => vv.AttributeValue != null && vv.AttributeValue.Attribute != null)
                        .Select(vv => new VariantAttributeDto
                        {
                            AttributeName = vv.AttributeValue.Attribute.Name,
                            AttributeValue = vv.AttributeValue.Value,
                            AttributeValueId = vv.AttributeValueId
                        })
                        .ToList(),
                    IsDeleted = v.IsDeleted,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    UpdatedBy = v.UpdatedBy,
                    CreatedBy = v.CreatedBy
                })
                .FirstOrDefaultAsync();

            return variant;
        }

        public async Task<ProductVariantDto> CreateAsync(ProductVariantCreateUpdateDto dto)
        {
            if (!await _productRepo.ExistsAsync(p => p.Id == dto.ProductId))
                throw new System.Exception("Sản phẩm không tồn tại");
            if (await _repo.ExistsAsync(v => v.Code == dto.Code))
                throw new System.Exception($"Mã biến thể '{dto.Code}' đã tồn tại");

            if (!string.IsNullOrEmpty(dto.SKU) && await _repo.ExistsAsync(v => v.SKU == dto.SKU))
                throw new System.Exception($"SKU '{dto.SKU}' đã tồn tại");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                ProductId = dto.ProductId,
                Name = dto.Name,
                SKU = dto.SKU,
                PriceAdjustment = dto.PriceAdjustment,
                StockQuantity = dto.StockQuantity,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(variant);
            await _repo.SaveChangesAsync();

            if (dto.AttributeValueIds?.Any() == true)
            {
                var variantAttrValues = dto.AttributeValueIds.Select(avId => new ProductVariantAttributeValue
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductVariantId = variant.Id,
                    AttributeValueId = avId
                }).ToList();

                await _variantAttrRepo.AddRangeAsync(variantAttrValues);
                await _variantAttrRepo.SaveChangesAsync();
            }

            await _logService.LogAsync(
                 code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "ProductVariants",
                entityId: variant.Id,
                description: $"Tạo mới biến thể sản phẩm {variant.Code} - {variant.Name}",
                oldValue: null,
                newValue: variant,
                userId: userId,
                 ip: ip,
                userAgent: agent
                );

            return (await GetByIdAsync(variant.Id))!;
        }

        public async Task<ProductVariantDto> UpdateAsync(string id, ProductVariantCreateUpdateDto dto)
        {
            var variant = await _repo.GetByIdAsync(id);
            if (variant == null)
                throw new System.Exception("Không tìm thấy biến thể sản phẩm");

            if (await _repo.ExistsAsync(p => p.Code == dto.Code, id))
                throw new System.Exception($"mã biến thể sản phẩm '{dto.Code}' đã tồn tại");

            if (!string.IsNullOrEmpty(dto.SKU) && await _repo.ExistsAsync(v => v.SKU == dto.SKU))
                throw new System.Exception($"SKU '{dto.SKU}' đã tồn tại");

            var oldValue = new { variant.Code, variant.Name, variant.StockQuantity };
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");
            }
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
            variant.Code = dto.Code;
            variant.ProductId = dto.ProductId;
            variant.Name = dto.Name;
            variant.SKU = dto.SKU;
            variant.PriceAdjustment = dto.PriceAdjustment;
            variant.StockQuantity = dto.StockQuantity;
            variant.UpdatedBy = userId;
            variant.UpdatedAt = DateTime.UtcNow;

            _repo.Update(variant);
            await _repo.SaveChangesAsync();

            await _variantAttrRepo.RemoveByVariantIdAsync(variant.Id);
            if (dto.AttributeValueIds?.Any() == true)
            {
                var variantAttrValues = dto.AttributeValueIds.Select(avId => new ProductVariantAttributeValue
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductVariantId = variant.Id,
                    AttributeValueId = avId
                }).ToList();

                await _variantAttrRepo.AddRangeAsync(variantAttrValues);
            }
            await _variantAttrRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "ProductVariants",
                entityId: variant.Id,
                description: $"Cập nhật biến thể sản phẩm {variant.Code} - {variant.Name}",
                oldValue: oldValue,
                newValue: variant,
                userId: userId,
                 ip: ip,
                userAgent: agent
                );

            return (await GetByIdAsync(variant.Id))!;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var variant = await _repo.GetByIdAsync(id);
            if (variant == null) return false;

            var oldValue = new
            {
                variant.IsDeleted,
                variant.UpdatedAt
            };

            variant.IsDeleted = true;
            variant.UpdatedAt = DateTime.UtcNow;
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
                entityType: "ProductVariants",
                entityId: variant.Id,
                description: $"Ngưng hoạt động biến thể sản phẩm {variant.Code} - {variant.Name}",
                oldValue: oldValue,
                newValue: new { variant.IsDeleted, variant.UpdatedAt },
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
                entityType: "ProductVariants",
                entityId: product.Id,
                description: $"Kích hoạt biến thể sản phẩm {product.Code} - {product.Name}",
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