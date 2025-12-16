using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs;
using Quanlicuahang.DTOs.Warehouse;
using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IWarehouseAreaService
    {
        Task<object> GetAllAsync(WarehouseAreaSearchDto searchDto);
        Task<WarehouseAreaDto?> GetByIdAsync(string id);
        Task<WarehouseAreaDto> CreateAsync(WarehouseAreaCreateUpdateDto dto);
        Task<bool> UpdateAsync(string id, WarehouseAreaCreateUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<object> GetSelectBoxAsync();
        Task<bool> TransferProductAsync(WarehouseAreaTransferDto dto);
        Task<object> GetWarehouseAreasByProductIdAsync(string productId);
    }

    public class WarehouseAreaService : IWarehouseAreaService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly IWarehouseAreaRepository _repo;
        private readonly IAreaInventoryRepository _areaInventoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly ApplicationDbContext _context;

        public WarehouseAreaService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            IWarehouseAreaRepository repo,
            IAreaInventoryRepository areaInventoryRepo,
            IProductRepository productRepo,
            ApplicationDbContext context
        )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repo = repo;
            _areaInventoryRepo = areaInventoryRepo;
            _productRepo = productRepo;
            _context = context;
        }

        public async Task<object> GetAllAsync(WarehouseAreaSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            IQueryable<WarehouseArea> query = _repo.GetAll(true);

            if (searchDto.Where != null)
            {
                var where = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(where.Code))
                {
                    var code = where.Code.Trim().ToLower();
                    query = query.Where(x => x.Code.ToLower().Contains(code));
                }
                if (!string.IsNullOrWhiteSpace(where.Name))
                {
                    var name = where.Name.Trim().ToLower();
                    query = query.Where(x => x.Name.ToLower().Contains(name));
                }
                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(x => x.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .Include(x => x.AreaInventories)
                    .ThenInclude(ai => ai.Product)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new WarehouseAreaDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    WarehouseId = "",
                    WarehouseName = null,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    IsDeleted = x.IsDeleted,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !x.IsDeleted,
                    isCanDeActive = !x.IsDeleted && !x.AreaInventories.Any(ai => !ai.IsDeleted && ai.Quantity > 0),
                    isCanActive = x.IsDeleted,
                    isCanDelete = !x.AreaInventories.Any(ai => !ai.IsDeleted && ai.Quantity > 0),
                    Products = x.AreaInventories
                        .Where(ai => !ai.IsDeleted && ai.Quantity > 0)
                        .Select(ai => new WarehouseAreaProductDto
                        {
                            ProductId = ai.ProductId,
                            ProductCode = ai.Product != null ? ai.Product.Code : null,
                            ProductName = ai.Product != null ? ai.Product.Name : null,
                            Quantity = ai.Quantity
                        })
                        .ToList()
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<WarehouseAreaDto?> GetByIdAsync(string id)
        {
            return await _repo.GetAll(true)
                .Include(x => x.AreaInventories)
                    .ThenInclude(ai => ai.Product)
                .Where(x => x.Id == id)
                .Select(x => new WarehouseAreaDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    WarehouseId = "",
                    WarehouseName = null,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    IsDeleted = x.IsDeleted,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !x.IsDeleted,
                    isCanDeActive = !x.IsDeleted && !x.AreaInventories.Any(ai => !ai.IsDeleted && ai.Quantity > 0),
                    isCanActive = x.IsDeleted,
                    isCanDelete = !x.AreaInventories.Any(ai => !ai.IsDeleted && ai.Quantity > 0),
                    Products = x.AreaInventories
                        .Where(ai => !ai.IsDeleted && ai.Quantity > 0)
                        .Select(ai => new WarehouseAreaProductDto
                        {
                            ProductId = ai.ProductId,
                            ProductCode = ai.Product != null ? ai.Product.Code : null,
                            ProductName = ai.Product != null ? ai.Product.Name : null,
                            Quantity = ai.Quantity
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<WarehouseAreaDto> CreateAsync(WarehouseAreaCreateUpdateDto dto)
        {
            if (await _repo.ExistsAsync(x => x.Code == dto.Code && !x.IsDeleted))
                throw new System.Exception($"Mã khu vực '{dto.Code}' đã tồn tại!");
            if (await _repo.ExistsAsync(x => x.Name == dto.Name && !x.IsDeleted))
                throw new System.Exception($"Tên khu vực '{dto.Name}' đã tồn tại!");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var entity = new WarehouseArea
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(Guid.NewGuid().ToString(), "Create", "WarehouseArea", entity.Id,
                $"Tạo mới khu vực {entity.Code} - {entity.Name}", null, entity, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, WarehouseAreaCreateUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new System.Exception("Khu vực kho không tồn tại!");

            if (!string.Equals(entity.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                var codeExists = await _repo.ExistsAsync(x => x.Code == dto.Code && !x.IsDeleted, excludeId: id);
                if (codeExists) throw new System.Exception($"Mã khu vực '{dto.Code}' đã tồn tại!");
            }

            if (!string.Equals(entity.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = await _repo.ExistsAsync(
                    x => x.Name == dto.Name && !x.IsDeleted,
                    excludeId: id
                );
                if (nameExists) throw new System.Exception($"Tên khu vực '{dto.Name}' đã tồn tại!");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var oldValue = new { entity.Code, entity.Name };
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            _repo.Update(entity);
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(Guid.NewGuid().ToString(), "Update", "WarehouseArea", entity.Id,
                $"Cập nhật khu vực {entity.Code} - {entity.Name}", oldValue, entity, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return true;
        }

        public async Task<bool> DeActiveAsync(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            // Chỉ cho phép ngưng hoạt động khi khu vực không còn sản phẩm (qty > 0)
            var hasStock = await _areaInventoryRepo.GetAll(false)
                .AnyAsync(ai => ai.WarehouseAreaId == id && !ai.IsDeleted && ai.Quantity > 0);
            if (hasStock)
                throw new System.Exception("Không thể ngưng hoạt động khu vực kho vì vẫn còn sản phẩm trong khu vực này.");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var oldValue = new { entity.IsDeleted };
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(Guid.NewGuid().ToString(), "DeActive", "WarehouseArea", entity.Id,
                $"Ngưng hoạt động khu vực {entity.Code} - {entity.Name}", oldValue, new { entity.IsDeleted }, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var oldValue = new { entity.IsDeleted };
            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            await _repo.SaveChangesAsync();

            await _logService.LogAsync(Guid.NewGuid().ToString(), "Active", "WarehouseArea", entity.Id,
                $"Kích hoạt khu vực {entity.Code} - {entity.Name}", oldValue, new { entity.IsDeleted }, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            // Chỉ cho phép xóa khi khu vực không còn sản phẩm (qty > 0)
            var hasStock = await _areaInventoryRepo.GetAll(false)
                .AnyAsync(ai => ai.WarehouseAreaId == id && ai.Quantity > 0);
            if (hasStock)
                throw new System.Exception("Không thể xóa khu vực kho vì vẫn còn sản phẩm trong khu vực này.");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var oldValue = new { entity.Id, entity.Code, entity.Name, entity.IsDeleted };

            // Hard delete: AreaInventory có cascade delete; StockEntryItem có FK set null
            _context.WarehouseAreas.Remove(entity);
            await _context.SaveChangesAsync();

            await _logService.LogAsync(Guid.NewGuid().ToString(), "Delete", "WarehouseArea", id,
                $"Xóa khu vực {oldValue.Code} - {oldValue.Name}", oldValue, null, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return true;
        }

        public async Task<object> GetSelectBoxAsync()
        {
            var query = _repo.GetAll(false)
                .OrderBy(x => x.Name)
                .Select(x => new SelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                });

            var data = await query.ToListAsync();
            var total = data.Count;
            return new { data, total };
        }

        public async Task<bool> TransferProductAsync(WarehouseAreaTransferDto dto)
        {
            if (dto == null) throw new System.Exception("Dữ liệu không hợp lệ");
            if (string.IsNullOrWhiteSpace(dto.ProductId)) throw new System.Exception("Thiếu sản phẩm");
            if (string.IsNullOrWhiteSpace(dto.FromWarehouseAreaId)) throw new System.Exception("Thiếu khu vực nguồn");
            if (string.IsNullOrWhiteSpace(dto.ToWarehouseAreaId)) throw new System.Exception("Thiếu khu vực đích");
            if (dto.FromWarehouseAreaId == dto.ToWarehouseAreaId) throw new System.Exception("Khu vực nguồn và đích phải khác nhau");
            if (dto.Quantity <= 0) throw new System.Exception("Số lượng chuyển phải > 0");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var fromArea = await _repo.GetByIdAsync(dto.FromWarehouseAreaId);
            var toArea = await _repo.GetByIdAsync(dto.ToWarehouseAreaId);
            if (fromArea == null || fromArea.IsDeleted) throw new System.Exception("Khu vực nguồn không tồn tại hoặc đã ngưng hoạt động");
            if (toArea == null || toArea.IsDeleted) throw new System.Exception("Khu vực đích không tồn tại hoặc đã ngưng hoạt động");

            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null || product.IsDeleted) throw new System.Exception("Sản phẩm không tồn tại hoặc đã bị vô hiệu hóa");

            // Lấy tồn kho tại khu vực nguồn
            var fromInv = await _areaInventoryRepo.GetAll(true)
                .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == dto.FromWarehouseAreaId
                                           && ai.ProductId == dto.ProductId
                                           && !ai.IsDeleted);
            if (fromInv == null || fromInv.Quantity <= 0)
                throw new System.Exception("Khu vực nguồn không có sản phẩm này");
            if (fromInv.Quantity < dto.Quantity)
                throw new System.Exception($"Số lượng trong khu vực nguồn không đủ. Còn {fromInv.Quantity}");

            // Trừ ở nguồn
            fromInv.Quantity = Math.Max(0, fromInv.Quantity - dto.Quantity);
            fromInv.UpdatedAt = DateTime.UtcNow;
            fromInv.UpdatedBy = userId;
            if (fromInv.Quantity <= 0)
            {
                // Tự động xóa khỏi khu vực khi về 0
                fromInv.IsDeleted = true;
            }
            _areaInventoryRepo.Update(fromInv);

            // Cộng ở đích (khôi phục nếu bản ghi đã bị xóa)
            var toInv = await _areaInventoryRepo.GetAll(true)
                .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == dto.ToWarehouseAreaId
                                           && ai.ProductId == dto.ProductId);
            if (toInv == null)
            {
                toInv = new AreaInventory
                {
                    Id = Guid.NewGuid().ToString(),
                    WarehouseAreaId = dto.ToWarehouseAreaId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    IsDeleted = false
                };
                await _areaInventoryRepo.AddAsync(toInv);
            }
            else
            {
                toInv.IsDeleted = false;
                toInv.Quantity = Math.Max(0, toInv.Quantity) + dto.Quantity;
                toInv.UpdatedAt = DateTime.UtcNow;
                toInv.UpdatedBy = userId;
                _areaInventoryRepo.Update(toInv);
            }

            await _areaInventoryRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Transfer",
                entityType: "AreaInventory",
                entityId: dto.ProductId,
                description: $"Chuyển {dto.Quantity} sản phẩm {product.Code} từ {fromArea.Code} sang {toArea.Code}",
                oldValue: new { From = dto.FromWarehouseAreaId, To = dto.ToWarehouseAreaId },
                newValue: dto,
                userId: userId,
                ip: _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                userAgent: _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString()
            );

            return true;
        }

        public async Task<object> GetWarehouseAreasByProductIdAsync(string productId)
        {
            // Lấy danh sách khu vực kho mà sản phẩm đã có tồn kho
            var areaInventories = await _areaInventoryRepo.GetAll(false)
                .Include(ai => ai.WarehouseArea)
                .Where(ai => ai.ProductId == productId && ai.Quantity > 0)
                .OrderByDescending(ai => ai.Quantity)
                .Select(ai => new
                {
                    WarehouseAreaId = ai.WarehouseAreaId,
                    WarehouseAreaCode = ai.WarehouseArea != null ? ai.WarehouseArea.Code : "",
                    WarehouseAreaName = ai.WarehouseArea != null ? ai.WarehouseArea.Name : "",
                    Quantity = ai.Quantity
                })
                .ToListAsync();

            return new { data = areaInventories };
        }
    }
}


