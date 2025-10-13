using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IProductAttributeValueService
    {
        Task<object> GetAllAsync(ProductAttributeValueSearchDto searchDto);
        Task<ProductAttributeValueDto?> GetByIdAsync(string id);
        Task<ProductAttributeValueDto> CreateAsync(string productId, ProductAttributeValueCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, ProductAttributeValueCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<List<ProductAttributeValueDto>> GetByProductIdAsync(string productId);
        Task<List<ProductAttributeValueDto>> GetByAttributeIdAsync(string attributeId);
    }

    public class ProductAttributeValueService : IProductAttributeValueService
    {
        private readonly IProductAttributeValueRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly IProductAttributeRepository _attributeRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public ProductAttributeValueService(
            IProductAttributeValueRepository repo,
            IProductRepository productRepo,
            IProductAttributeRepository attributeRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _repo = repo;
            _productRepo = productRepo;
            _attributeRepo = attributeRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(ProductAttributeValueSearchDto searchDto)
        {
            var query = _repo.GetAll(true);

            if (searchDto.Where != null)
            {
                var where = searchDto.Where;

                if (!string.IsNullOrWhiteSpace(where.AttributeId))
                    query = query.Where(av => av.AttributeId == where.AttributeId);

                if (!string.IsNullOrWhiteSpace(where.ValueString))
                    query = query.Where(av => av.ValueString != null &&
                        av.ValueString.ToLower().Contains(where.ValueString.Trim().ToLower()));

                if (where.ValueDecimal.HasValue)
                    query = query.Where(av => av.ValueDecimal == where.ValueDecimal.Value);

                if (where.ValueInt.HasValue)
                    query = query.Where(av => av.ValueInt == where.ValueInt.Value);

                if (where.ValueBool.HasValue)
                    query = query.Where(av => av.ValueBool == where.ValueBool.Value);

                if (where.ValueDate.HasValue)
                    query = query.Where(av => av.ValueDate.HasValue &&
                        av.ValueDate.Value.Date == where.ValueDate.Value.Date);

                if (where.IsDeleted.HasValue)
                    query = query.Where(av => av.IsDeleted == where.IsDeleted.Value);
            }

            var total = await query.CountAsync();

            var data = await query
                .Include(av => av.Attribute)
                .Include(av => av.Product)
                .OrderBy(av => av.DisplayOrder)
                .ThenByDescending(av => av.CreatedAt)
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
                    UpdatedAt = av.UpdatedAt,
                    isCanEdit = !av.IsDeleted,
                    isCanDeActive = !av.IsDeleted,
                    isCanActive = av.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<ProductAttributeValueDto?> GetByIdAsync(string id)
        {
            var av = await _repo.GetAll(true)
                .Include(a => a.Attribute)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (av == null) return null;

            return new ProductAttributeValueDto
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
                UpdatedAt = av.UpdatedAt,
                isCanEdit = !av.IsDeleted,
                isCanDeActive = !av.IsDeleted,
                isCanActive = av.IsDeleted
            };
        }

        public async Task<ProductAttributeValueDto> CreateAsync(string productId, ProductAttributeValueCreateUpdateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new System.Exception("Sản phẩm không tồn tại");

            var attribute = await _attributeRepo.GetByIdAsync(dto.AttributeId);
            if (attribute == null)
                throw new System.Exception("Thuộc tính không tồn tại");

            var existing = await _repo.GetAll(true)
                .FirstOrDefaultAsync(av => av.ProductId == productId && av.AttributeId == dto.AttributeId);

            if (existing != null && !existing.IsDeleted)
                throw new System.Exception($"Sản phẩm đã có thuộc tính '{attribute.Name}'");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            ProductAttributeValue attrValue;

            if (existing != null && existing.IsDeleted)
            {
                attrValue = existing;
                attrValue.ValueString = dto.ValueString;
                attrValue.ValueDecimal = dto.ValueDecimal;
                attrValue.ValueInt = dto.ValueInt;
                attrValue.ValueBool = dto.ValueBool;
                attrValue.ValueDate = dto.ValueDate;
                attrValue.DisplayOrder = dto.DisplayOrder;
                attrValue.IsDeleted = false;
                attrValue.UpdatedAt = DateTime.UtcNow;
                attrValue.UpdatedBy = userId;

                _repo.Update(attrValue);
            }
            else
            {
                attrValue = new ProductAttributeValue
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = productId,
                    AttributeId = dto.AttributeId,
                    ValueString = dto.ValueString,
                    ValueDecimal = dto.ValueDecimal,
                    ValueInt = dto.ValueInt,
                    ValueBool = dto.ValueBool,
                    ValueDate = dto.ValueDate,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };
                await _repo.AddAsync(attrValue);
            }

            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: existing != null ? "Reactive" : "Create",
                entityType: "ProductAttributeValues",
                entityId: attrValue.Id,
                description: existing != null
                    ? $"Kích hoạt lại giá trị thuộc tính {attribute.Name} cho sản phẩm {product.Name}"
                    : $"Tạo mới giá trị thuộc tính {attribute.Name} cho sản phẩm {product.Name}",
                oldValue: existing != null ? existing : null,
                newValue: attrValue,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(attrValue.Id))!;
        }
        public async Task<bool> UpdateAsync(string id, ProductAttributeValueCreateUpdateDto dto)
        {
            var av = await _repo.GetByIdAsync(id);
            if (av == null) throw new System.Exception("Giá trị thuộc tính không tồn tại");

            var attribute = await _attributeRepo.GetByIdAsync(dto.AttributeId);
            if (attribute == null)
                throw new System.Exception("Thuộc tính không tồn tại");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new
            {
                av.AttributeId,
                av.ValueString,
                av.ValueDecimal,
                av.ValueInt,
                av.ValueBool,
                av.ValueDate,
                av.DisplayOrder
            };

            av.AttributeId = dto.AttributeId;
            av.ValueString = dto.ValueString;
            av.ValueDecimal = dto.ValueDecimal;
            av.ValueInt = dto.ValueInt;
            av.ValueBool = dto.ValueBool;
            av.ValueDate = dto.ValueDate;
            av.DisplayOrder = dto.DisplayOrder;
            av.UpdatedAt = DateTime.UtcNow;
            av.UpdatedBy = userId;

            _repo.Update(av);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "ProductAttributeValues",
                entityId: av.Id,
                description: $"Cập nhật giá trị thuộc tính {attribute.Name} cho sản phẩm {av.ProductId}",
                oldValue: oldValue,
                newValue: av,
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var av = await _repo.GetByIdAsync(id);
            if (av == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { av.IsDeleted, av.UpdatedAt };

            av.IsDeleted = true;
            av.UpdatedAt = DateTime.UtcNow;
            av.UpdatedBy = userId;

            _repo.Update(av);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "ProductAttributeValues",
                entityId: av.Id,
                description: $"Ngưng hoạt động giá trị thuộc tính {av.AttributeId} cho sản phẩm {av.ProductId}",
                oldValue: oldValue,
                newValue: new { av.IsDeleted, av.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var av = await _repo.GetByIdAsync(id);
            if (av == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var oldValue = new { av.IsDeleted, av.UpdatedAt };

            av.IsDeleted = false;
            av.UpdatedAt = DateTime.UtcNow;
            av.UpdatedBy = userId;

            _repo.Update(av);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "ProductAttributeValues",
                entityId: av.Id,
                description: $"Kích hoạt giá trị thuộc tính {av.AttributeId} cho sản phẩm {av.ProductId}",
                oldValue: oldValue,
                newValue: new { av.IsDeleted, av.UpdatedAt },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<List<ProductAttributeValueDto>> GetByProductIdAsync(string productId)
        {
            var query = _repo.GetAll(true)
                .Where(av => av.ProductId == productId)
                .Include(av => av.Attribute)
                .Include(av => av.Product);

            var data = await query.Select(av => new ProductAttributeValueDto
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
                UpdatedAt = av.UpdatedAt,
                isCanEdit = !av.IsDeleted,
                isCanDeActive = !av.IsDeleted,
                isCanActive = av.IsDeleted
            }).ToListAsync();

            return data;
        }

        public async Task<List<ProductAttributeValueDto>> GetByAttributeIdAsync(string attributeId)
        {
            var query = _repo.GetAll(true)
                .Where(av => av.AttributeId == attributeId)
                .Include(av => av.Attribute)
                .Include(av => av.Product);

            var data = await query.Select(av => new ProductAttributeValueDto
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
                UpdatedAt = av.UpdatedAt,
                isCanEdit = !av.IsDeleted,
                isCanDeActive = !av.IsDeleted,
                isCanActive = av.IsDeleted
            }).ToListAsync();

            return data;
        }

    }
}
