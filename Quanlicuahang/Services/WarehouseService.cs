using Microsoft.EntityFrameworkCore;
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
        Task<object> GetSelectBoxAsync();
    }

    public class WarehouseAreaService : IWarehouseAreaService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly IWarehouseAreaRepository _repo;

        public WarehouseAreaService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            IWarehouseAreaRepository repo
        )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repo = repo;
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
                    isCanDeActive = !x.IsDeleted,
                    isCanActive = x.IsDeleted,
                    Products = x.AreaInventories
                        .Where(ai => !ai.IsDeleted)
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
                    Products = x.AreaInventories
                        .Where(ai => !ai.IsDeleted)
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
    }
}


