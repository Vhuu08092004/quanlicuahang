using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Product;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
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
        private readonly IAreaInventoryRepository _areaInventoryRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public ProductService(
            IProductRepository repo,
            IProductAttributeRepository attributeRepo,
            IProductAttributeValueRepository attributeValueRepo,
            IAreaInventoryRepository areaInventoryRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _repo = repo;
            _attributeRepo = attributeRepo;
            _attributeValueRepo = attributeValueRepo;
            _areaInventoryRepo = areaInventoryRepo;
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

            var products = await query
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Unit = p.Unit,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            // Lấy tổng số lượng tồn kho từ AreaInventory cho các sản phẩm
            var productIds = products.Select(p => p.Id).ToList();
            var quantityDict = await GetProductsQuantityAsync(productIds);

            // Map sang ProductDto với số lượng tồn kho
            var data = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                Unit = p.Unit,
                Quantity = quantityDict.ContainsKey(p.Id) ? quantityDict[p.Id] : 0,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                SupplierId = p.SupplierId,
                SupplierName = p.SupplierName,
                IsDeleted = p.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                isCanView = true,
                isCanCreate = true,
                isCanEdit = !p.IsDeleted,
                isCanDeActive = !p.IsDeleted,
                isCanActive = p.IsDeleted
            }).ToList();

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
                .Select(p => new
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Unit = p.Unit,
                    ImageUrl = p.ImageUrl,
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

            if (product == null) return null;

            // Lấy tổng số lượng tồn kho từ AreaInventory
            var totalQuantity = await GetProductQuantityAsync(product.Id);

            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                Barcode = product.Barcode,
                Price = product.Price,
                Unit = product.Unit,
                Quantity = totalQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.CategoryName,
                SupplierId = product.SupplierId,
                SupplierName = product.SupplierName,
                Attributes = product.Attributes,
                IsDeleted = product.IsDeleted,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto> CreateAsync(ProductCreateUpdateDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new System.Exception("Mã sản phẩm không được để trống!");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new System.Exception("Tên sản phẩm không được để trống!");

            if (dto.Price < 0)
                throw new System.Exception("Giá sản phẩm phải lớn hơn hoặc bằng 0!");

            if (string.IsNullOrWhiteSpace(dto.Unit))
                throw new System.Exception("Đơn vị tính không được để trống!");

            if (await _repo.ExistsAsync(p => p.Code == dto.Code))
                throw new System.Exception($"Mã sản phẩm '{dto.Code}' đã tồn tại!");

            if (await _repo.ExistsAsync(p => p.Barcode == dto.Barcode))
                throw new System.Exception($"Barcode sản phẩm '{dto.Barcode}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : dto.Barcode.Trim(),
                Price = dto.Price,
                Unit = dto.Unit.Trim(),
                ImageUrl = dto.ImageUrl,
                CategoryId = string.IsNullOrWhiteSpace(dto.CategoryId) ? null : dto.CategoryId,
                SupplierId = string.IsNullOrWhiteSpace(dto.SupplierId) ? null : dto.SupplierId,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            await _repo.SaveChangesAsync();


            // Create product attribute values if provided
            if (dto.Attributes != null && dto.Attributes.Count > 0)
            {
                var attributeValues = new List<ProductAttributeValue>();

                foreach (var attrDto in dto.Attributes)
                {
                    var attributeExists = await _attributeRepo.ExistsAsync(a => a.Id == attrDto.AttributeId && !a.IsDeleted);
                    if (!attributeExists)
                        throw new System.Exception($"Thuộc tính với ID '{attrDto.AttributeId}' không tồn tại!");

                    var attributeValue = new ProductAttributeValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = product.Id,
                        AttributeId = attrDto.AttributeId,
                        ValueString = attrDto.ValueString,
                        ValueDecimal = attrDto.ValueDecimal,
                        ValueInt = attrDto.ValueInt,
                        ValueBool = attrDto.ValueBool,
                        ValueDate = attrDto.ValueDate,
                        DisplayOrder = attrDto.DisplayOrder,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    attributeValues.Add(attributeValue);
                }

                foreach (var attrValue in attributeValues)
                {
                    await _attributeValueRepo.AddAsync(attrValue);
                }
                await _attributeValueRepo.SaveChangesAsync();
            }

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

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new System.Exception("Mã sản phẩm không được để trống!");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new System.Exception("Tên sản phẩm không được để trống!");

            if (dto.Price < 0)
                throw new System.Exception("Giá sản phẩm phải lớn hơn hoặc bằng 0!");

            if (string.IsNullOrWhiteSpace(dto.Unit))
                throw new System.Exception("Đơn vị tính không được để trống!");

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
                product.ImageUrl,
                product.CategoryId,
                product.SupplierId,
            };



            product.Code = dto.Code.Trim();
            product.Name = dto.Name.Trim();
            product.Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : dto.Barcode.Trim();
            product.Price = dto.Price;
            product.Unit = dto.Unit.Trim();
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = string.IsNullOrWhiteSpace(dto.CategoryId) ? null : dto.CategoryId;
            product.SupplierId = string.IsNullOrWhiteSpace(dto.SupplierId) ? null : dto.SupplierId;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;

            _repo.Update(product);
            await _repo.SaveChangesAsync();

            // Update product attribute values
            if (dto.Attributes != null)
            {
                var existingAttributeValues = await _attributeValueRepo.GetAll(false)
                    .Where(av => av.ProductId == id)
                    .ToListAsync();

                foreach (var existing in existingAttributeValues)
                {
                    existing.IsDeleted = true;
                    existing.UpdatedBy = userId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _attributeValueRepo.Update(existing);
                }

                foreach (var attrDto in dto.Attributes)
                {
                    var attributeExists = await _attributeRepo.ExistsAsync(a => a.Id == attrDto.AttributeId && !a.IsDeleted);
                    if (!attributeExists)
                        throw new System.Exception($"Thuộc tính với ID '{attrDto.AttributeId}' không tồn tại!");

                    var existingAttrValue = existingAttributeValues
                        .FirstOrDefault(av => av.AttributeId == attrDto.AttributeId);

                    if (existingAttrValue != null)
                    {
                        existingAttrValue.ValueString = attrDto.ValueString;
                        existingAttrValue.ValueDecimal = attrDto.ValueDecimal;
                        existingAttrValue.ValueInt = attrDto.ValueInt;
                        existingAttrValue.ValueBool = attrDto.ValueBool;
                        existingAttrValue.ValueDate = attrDto.ValueDate;
                        existingAttrValue.DisplayOrder = attrDto.DisplayOrder;
                        existingAttrValue.IsDeleted = false;
                        existingAttrValue.UpdatedBy = userId;
                        existingAttrValue.UpdatedAt = DateTime.UtcNow;
                        _attributeValueRepo.Update(existingAttrValue);
                    }
                    else
                    {
                        var attributeValue = new ProductAttributeValue
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = id,
                            AttributeId = attrDto.AttributeId,
                            ValueString = attrDto.ValueString,
                            ValueDecimal = attrDto.ValueDecimal,
                            ValueInt = attrDto.ValueInt,
                            ValueBool = attrDto.ValueBool,
                            ValueDate = attrDto.ValueDate,
                            DisplayOrder = attrDto.DisplayOrder,
                            CreatedBy = userId,
                            UpdatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _attributeValueRepo.AddAsync(attributeValue);
                    }
                }

                await _attributeValueRepo.SaveChangesAsync();
            }

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
            // Lấy danh sách sản phẩm
            var products = await _repo.GetAll(false)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name
                })
                .ToListAsync();

            // Lấy tổng số lượng tồn kho từ AreaInventory cho mỗi sản phẩm
            var inventoryQuantities = await _areaInventoryRepo.GetAll(false)
                .GroupBy(ai => ai.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(ai => ai.Quantity)
                })
                .ToListAsync();

            // Tạo dictionary để tra cứu nhanh
            var quantityDict = inventoryQuantities.ToDictionary(x => x.ProductId, x => x.TotalQuantity);

            // Kết hợp dữ liệu
            var data = products.Select(p => new
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Quantity = quantityDict.ContainsKey(p.Id) ? quantityDict[p.Id] : 0
            }).ToList();

            var total = data.Count;

            return new { data, total };
        }

        // Lấy tổng số lượng tồn kho của sản phẩm từ AreaInventory
        public async Task<int> GetProductQuantityAsync(string productId)
        {
            var totalQuantity = await _areaInventoryRepo.GetAll(false)
                .Where(ai => ai.ProductId == productId)
                .SumAsync(ai => (int?)ai.Quantity) ?? 0;

            return totalQuantity;
        }

        // Lấy tổng số lượng tồn kho của nhiều sản phẩm từ AreaInventory
        public async Task<Dictionary<string, int>> GetProductsQuantityAsync(List<string> productIds)
        {
            var inventoryQuantities = await _areaInventoryRepo.GetAll(false)
                .Where(ai => productIds.Contains(ai.ProductId))
                .GroupBy(ai => ai.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(ai => ai.Quantity)
                })
                .ToListAsync();

            var quantityDict = inventoryQuantities.ToDictionary(x => x.ProductId, x => x.TotalQuantity);

            // Đảm bảo tất cả productIds đều có trong dictionary (với giá trị 0 nếu không có tồn kho)
            foreach (var productId in productIds)
            {
                if (!quantityDict.ContainsKey(productId))
                {
                    quantityDict[productId] = 0;
                }
            }

            return quantityDict;
        }
    }
}