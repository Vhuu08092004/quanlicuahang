using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.StockEntry;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IStockEntryService
    {
        Task<object> GetAllAsync(StockEntrySearchDto searchDto);
        Task<StockEntryDto?> GetByIdAsync(string id);
        Task<StockEntryDto> CreateAsync(StockEntryCreateDto dto);
        Task<StockEntryDto> UpdateAsync(string id, StockEntryUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class StockEntryService : IStockEntryService
    {
        private readonly IStockEntryRepository _stockEntryRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAreaInventoryRepository _areaInventoryRepo;
        private readonly IWarehouseAreaRepository _warehouseAreaRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IUserRepository _userRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public StockEntryService(
            IStockEntryRepository stockEntryRepo,
            IInventoryRepository inventoryRepo,
            IProductRepository productRepo,
            IAreaInventoryRepository areaInventoryRepo,
            IWarehouseAreaRepository warehouseAreaRepo,
            ISupplierRepository supplierRepo,
            IUserRepository userRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _stockEntryRepo = stockEntryRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _areaInventoryRepo = areaInventoryRepo;
            _warehouseAreaRepo = warehouseAreaRepo;
            _supplierRepo = supplierRepo;
            _userRepo = userRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(StockEntrySearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _stockEntryRepo.GetAll(true)
                .Include(se => se.Supplier)
                .Include(se => se.User)
                .AsQueryable();

            if (searchDto.Where != null)
            {
                var w = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(w.Code))
                {
                    var code = w.Code.Trim().ToLower();
                    query = query.Where(o => o.Code.ToLower().Contains(code));
                }
                if (!string.IsNullOrWhiteSpace(w.SupplierName))
                {
                    var sname = w.SupplierName.Trim().ToLower();
                    query = query.Where(o => o.Supplier != null && o.Supplier.Name.ToLower().Contains(sname));
                }
                if (!string.IsNullOrWhiteSpace(w.Status))
                {
                    var st = w.Status.Trim().ToLower();
                    query = query.Where(o => o.Status.ToLower() == st);
                }
                if (w.FromDate.HasValue)
                {
                    var from = w.FromDate.Value.Date;
                    query = query.Where(o => o.EntryDate >= from);
                }
                if (w.ToDate.HasValue)
                {
                    var to = w.ToDate.Value.Date.AddDays(1);
                    query = query.Where(o => o.EntryDate < to);
                }
                if (w.IsDeleted.HasValue)
                {
                    query = query.Where(o => o.IsDeleted == w.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(o => new StockEntryDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    SupplierId = o.SupplierId,
                    SupplierName = o.Supplier != null ? o.Supplier.Name : null,
                    EntryDate = o.EntryDate,
                    Status = o.Status,
                    TotalCost = o.TotalCost,
                    Note = o.Note,
                    CreatedAt = o.CreatedAt,
                    CreatedByName = o.User != null ? o.User.FullName : null,
                    IsDeleted = o.IsDeleted,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = o.Status.ToLower() == "pending" && !o.IsDeleted,
                    isCanDeActive = !o.IsDeleted,
                    isCanActive = o.IsDeleted,
                    isCanUpdateStatus = !o.IsDeleted,
                    isCanCancel = o.Status.ToLower() == "pending" && !o.IsDeleted,
                    isCanComplete = o.Status.ToLower() == "pending" && !o.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<StockEntryDto?> GetByIdAsync(string id)
        {
            var entry = await _stockEntryRepo.GetAll(true)
                .Include(se => se.Supplier)
                .Include(se => se.User)
                .Include(se => se.StockEntryItems)
                    .ThenInclude(sei => sei.Product)
                .Include(se => se.StockEntryItems)
                    .ThenInclude(sei => sei.WarehouseArea)
                .Where(o => o.Id == id)
                .Select(o => new StockEntryDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    SupplierId = o.SupplierId,
                    SupplierName = o.Supplier != null ? o.Supplier.Name : null,
                    EntryDate = o.EntryDate,
                    Status = o.Status,
                    TotalCost = o.TotalCost,
                    Note = o.Note,
                    CreatedAt = o.CreatedAt,
                    CreatedByName = o.User != null ? (o.User.FullName ?? o.User.Username) : null,
                    IsDeleted = o.IsDeleted,
                    Items = o.StockEntryItems
                        .Where(i => !i.IsDeleted)
                        .Select(i => new StockEntryItemDto
                        {
                            ProductId = i.ProductId,
                            ProductCode = i.Product != null ? i.Product.Code : null,
                            ProductName = i.Product != null ? i.Product.Name : null,
                            Quantity = i.Quantity,
                            UnitCost = i.UnitCost,
                            WarehouseAreaId = i.WarehouseAreaId,
                            WarehouseAreaName = i.WarehouseArea != null ? i.WarehouseArea.Name : null
                        }).ToList()
                })
                .FirstOrDefaultAsync();
            return entry;
        }

        public async Task<StockEntryDto> CreateAsync(StockEntryCreateDto dto)
        {
            // Kiểm tra nhà cung cấp bắt buộc
            if (string.IsNullOrWhiteSpace(dto.SupplierId))
                throw new System.Exception("Vui lòng chọn nhà cung cấp");

            // Kiểm tra nhà cung cấp có tồn tại không
            var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
            if (supplier == null || supplier.IsDeleted)
                throw new System.Exception("Nhà cung cấp không tồn tại hoặc đã bị xóa");

            if (dto.Items == null || dto.Items.Count == 0)
                throw new System.Exception("Phiếu nhập phải có ít nhất một sản phẩm");

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new System.Exception("Số lượng phải > 0");
                if (item.UnitCost < 0)
                    throw new System.Exception("Giá nhập không hợp lệ");
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.IsDeleted)
                    throw new System.Exception($"Sản phẩm {item.ProductId} không tồn tại hoặc đã bị xóa");

                // Kiểm tra khu vực kho bắt buộc
                if (string.IsNullOrWhiteSpace(item.WarehouseAreaId))
                    throw new System.Exception($"Vui lòng chọn khu vực kho cho sản phẩm {product.Code ?? product.Name}");

                // Validate WarehouseAreaId
                var warehouseArea = await _warehouseAreaRepo.GetByIdAsync(item.WarehouseAreaId);
                if (warehouseArea == null || warehouseArea.IsDeleted)
                    throw new System.Exception($"Khu vực kho không tồn tại hoặc đã bị xóa");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();

            // Kiểm tra userId có tồn tại trong database không
            string? validUserId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user != null && !user.IsDeleted)
                {
                    validUserId = userId;
                }
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var totalCost = dto.Items.Sum(x => x.UnitCost * x.Quantity);

            var entry = new StockEntry
            {
                Id = Guid.NewGuid().ToString(),
                Code = GenerateCode("STENTRY"),
                SupplierId = string.IsNullOrWhiteSpace(dto.SupplierId) ? null : dto.SupplierId,
                EntryDate = DateTime.UtcNow,
                Status = "pending",
                TotalCost = totalCost,
                Note = dto.Note,
                UserId = validUserId,
                CreatedBy = validUserId ?? "system",
                UpdatedBy = validUserId ?? "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stockEntryRepo.AddAsync(entry);

            foreach (var item in dto.Items)
            {
                var sei = new StockEntryItem
                {
                    Id = Guid.NewGuid().ToString(),
                    StockEntryId = entry.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    Subtotal = item.UnitCost * item.Quantity,
                    WarehouseAreaId = string.IsNullOrWhiteSpace(item.WarehouseAreaId) ? null : item.WarehouseAreaId,
                    CreatedBy = validUserId ?? "system",
                    UpdatedBy = validUserId ?? "system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                entry.StockEntryItems.Add(sei);

                // Cập nhật AreaInventory nếu có WarehouseAreaId
                if (!string.IsNullOrWhiteSpace(item.WarehouseAreaId))
                {
                    var existingAreaInventory = await _areaInventoryRepo.GetAll(false)
                        .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == item.WarehouseAreaId && ai.ProductId == item.ProductId);

                    if (existingAreaInventory != null)
                    {
                        existingAreaInventory.Quantity += item.Quantity;
                        existingAreaInventory.UpdatedBy = validUserId ?? "system";
                        existingAreaInventory.UpdatedAt = DateTime.UtcNow;
                        _areaInventoryRepo.Update(existingAreaInventory);
                    }
                    else
                    {
                        var newAreaInventory = new AreaInventory
                        {
                            Id = Guid.NewGuid().ToString(),
                            WarehouseAreaId = item.WarehouseAreaId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            CreatedBy = validUserId ?? "system",
                            UpdatedBy = validUserId ?? "system",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _areaInventoryRepo.AddAsync(newAreaInventory);
                    }
                }
            }

            var grouped = dto.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // Load tất cả products cần update một lần để tránh vấn đề tracking
            var productIds = grouped.Select(g => g.ProductId).ToList();
            var products = await _productRepo.GetAll(false)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var productDict = products.ToDictionary(p => p.Id);
            foreach (var g in grouped)
            {
                if (productDict.TryGetValue(g.ProductId, out var prod))
                {
                    prod.Quantity = prod.Quantity + Math.Abs(g.Qty);
                    prod.UpdatedBy = validUserId ?? "system";
                    prod.UpdatedAt = DateTime.UtcNow;
                    _productRepo.Update(prod);
                }
            }

            await _stockEntryRepo.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                var stockIn = new Inventory
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = GenerateCode("INVIN"),
                    ProductId = item.ProductId,
                    Quantity = Math.Abs(item.Quantity),
                    CreatedBy = validUserId ?? "system",
                    UpdatedBy = validUserId ?? "system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _inventoryRepo.AddAsync(stockIn);
            }

            await _stockEntryRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "StockEntries",
                entityId: entry.Id,
                description: $"Tạo phiếu nhập {entry.Code}",
                oldValue: null,
                newValue: new
                {
                    entry.Id,
                    entry.Code,
                    entry.TotalCost,
                    Items = dto.Items.Select(x => new { x.ProductId, x.Quantity, x.UnitCost }).ToList()
                },
                userId: validUserId ?? "system",
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(entry.Id))!;
        }

        public async Task<StockEntryDto> UpdateAsync(string id, StockEntryUpdateDto dto)
        {
            var entry = await _stockEntryRepo.GetAll(true)
                .Include(se => se.StockEntryItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (entry == null) throw new System.Exception("Không tìm thấy phiếu nhập");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                var from = (entry.Status ?? "pending").Trim().ToLower();
                var to = dto.Status!.Trim().ToLower();

                var allowed =
                    (from == "pending" && (to == "completed" || to == "cancelled"));

                if (!allowed)
                    throw new System.Exception($"Chuyển trạng thái {from} -> {to} không hợp lệ.");

                if (to == "cancelled")
                {
                    var groups = entry.StockEntryItems.Where(i => !i.IsDeleted)
                        .GroupBy(i => i.ProductId)
                        .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                        .ToList();

                    // Load tất cả products cần update một lần để tránh vấn đề tracking
                    var productIds = groups.Select(g => g.ProductId).ToList();
                    var products = await _productRepo.GetAll(false)
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync();
                    var productDict = products.ToDictionary(p => p.Id);

                    foreach (var g in groups)
                    {
                        if (productDict.TryGetValue(g.ProductId, out var prod))
                        {
                            var decrease = Math.Abs(g.Qty);
                            if (prod.Quantity < decrease)
                                decrease = prod.Quantity;
                            prod.Quantity = prod.Quantity - decrease;
                            prod.UpdatedBy = userId;
                            prod.UpdatedAt = DateTime.UtcNow;
                            _productRepo.Update(prod);

                            var inv = new Inventory
                            {
                                Id = Guid.NewGuid().ToString(),
                                Code = GenerateCode("INVREV"),
                                ProductId = g.ProductId,
                                Quantity = -Math.Abs(g.Qty),
                                CreatedBy = userId,
                                UpdatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            await _inventoryRepo.AddAsync(inv);
                        }
                    }

                    // Trừ lại quantity từ AreaInventory khi hủy phiếu
                    foreach (var item in entry.StockEntryItems.Where(i => !i.IsDeleted && !string.IsNullOrWhiteSpace(i.WarehouseAreaId)))
                    {
                        var existingAreaInventory = await _areaInventoryRepo.GetAll(false)
                            .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == item.WarehouseAreaId && ai.ProductId == item.ProductId);

                        if (existingAreaInventory != null)
                        {
                            existingAreaInventory.Quantity = Math.Max(0, existingAreaInventory.Quantity - item.Quantity);
                            existingAreaInventory.UpdatedBy = userId;
                            existingAreaInventory.UpdatedAt = DateTime.UtcNow;
                            _areaInventoryRepo.Update(existingAreaInventory);
                        }
                    }
                }

                entry.Status = to;
                entry.UpdatedBy = userId;
                entry.UpdatedAt = DateTime.UtcNow;
                _stockEntryRepo.Update(entry);
                await _stockEntryRepo.SaveChangesAsync();

                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "UpdateStatus",
                    entityType: "StockEntries",
                    entityId: entry.Id,
                    description: $"Cập nhật trạng thái: {from} -> {to}",
                    oldValue: new { Status = from },
                    newValue: new { Status = to },
                    userId: userId,
                    ip: ip,
                    userAgent: agent
                );

                return (await GetByIdAsync(entry.Id))!;
            }

            if ((entry.Status ?? "").Trim().ToLower() != "pending")
                throw new System.Exception("Chỉ cho phép chỉnh sửa khi phiếu đang Pending");

            // Kiểm tra nhà cung cấp nếu được cập nhật
            if (!string.IsNullOrWhiteSpace(dto.SupplierId))
            {
                var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
                if (supplier == null || supplier.IsDeleted)
                    throw new System.Exception("Nhà cung cấp không tồn tại hoặc đã bị xóa");
            }
            // Nếu không truyền SupplierId nhưng phiếu chưa có SupplierId, bắt buộc phải chọn
            else if (string.IsNullOrWhiteSpace(entry.SupplierId))
            {
                throw new System.Exception("Vui lòng chọn nhà cung cấp");
            }

            if (dto.Items != null)
            {
                if (dto.Items.Count == 0)
                    throw new System.Exception("Phiếu nhập phải có ít nhất một sản phẩm");

                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                        throw new System.Exception("Số lượng phải > 0");
                    if (item.UnitCost < 0)
                        throw new System.Exception("Giá nhập không hợp lệ");
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product == null || product.IsDeleted)
                        throw new System.Exception($"Sản phẩm {item.ProductId} không tồn tại hoặc đã bị xóa");

                    // Kiểm tra khu vực kho bắt buộc
                    if (string.IsNullOrWhiteSpace(item.WarehouseAreaId))
                        throw new System.Exception($"Vui lòng chọn khu vực kho cho sản phẩm {product.Code ?? product.Name}");

                    // Validate WarehouseAreaId
                    var warehouseArea = await _warehouseAreaRepo.GetByIdAsync(item.WarehouseAreaId);
                    if (warehouseArea == null || warehouseArea.IsDeleted)
                        throw new System.Exception($"Khu vực kho không tồn tại hoặc đã bị xóa");
                }

                var oldGroups = entry.StockEntryItems
                    .Where(sei => !sei.IsDeleted)
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
                var newGroups = dto.Items
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                var affectedProductIds = oldGroups.Keys.Union(newGroups.Keys).Distinct().ToList();

                // Load tất cả products cần update một lần để tránh vấn đề tracking
                var productsToUpdate = await _productRepo.GetAll(false)
                    .Where(p => affectedProductIds.Contains(p.Id))
                    .ToListAsync();
                var productDict = productsToUpdate.ToDictionary(p => p.Id);

                foreach (var pid in affectedProductIds)
                {
                    var oldQty = oldGroups.ContainsKey(pid) ? oldGroups[pid] : 0;
                    var newQty = newGroups.ContainsKey(pid) ? newGroups[pid] : 0;
                    var delta = newQty - oldQty;

                    if (delta != 0)
                    {
                        if (!productDict.TryGetValue(pid, out var product))
                            throw new System.Exception($"Sản phẩm {pid} không tồn tại");

                        var newStock = product.Quantity + delta;
                        if (newStock < 0)
                            throw new System.Exception($"Điều chỉnh tồn kho vượt quá số lượng hiện có cho sản phẩm {pid}");

                        product.Quantity = newStock;
                        product.UpdatedBy = userId;
                        product.UpdatedAt = DateTime.UtcNow;
                        _productRepo.Update(product);

                        var inv = new Inventory
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = GenerateCode("INVADJ"),
                            ProductId = pid,
                            Quantity = delta,
                            CreatedBy = userId,
                            UpdatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _inventoryRepo.AddAsync(inv);
                    }
                }

                // Trừ lại quantity cũ từ AreaInventory trước khi xóa các item cũ (chỉ các items chưa bị xóa)
                foreach (var oldItem in entry.StockEntryItems.Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.WarehouseAreaId)))
                {
                    var existingAreaInventory = await _areaInventoryRepo.GetAll(false)
                        .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == oldItem.WarehouseAreaId && ai.ProductId == oldItem.ProductId);

                    if (existingAreaInventory != null)
                    {
                        existingAreaInventory.Quantity = Math.Max(0, existingAreaInventory.Quantity - oldItem.Quantity);
                        existingAreaInventory.UpdatedBy = userId;
                        existingAreaInventory.UpdatedAt = DateTime.UtcNow;
                        _areaInventoryRepo.Update(existingAreaInventory);
                    }
                }

                // Soft-delete toàn bộ items cũ (chỉ các items chưa bị xóa)
                foreach (var it in entry.StockEntryItems.Where(x => !x.IsDeleted))
                {
                    it.IsDeleted = true;
                    it.UpdatedBy = userId;
                    it.UpdatedAt = DateTime.UtcNow;
                }

                foreach (var item in dto.Items)
                {
                    var newItem = new StockEntryItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        StockEntryId = entry.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitCost,
                        Subtotal = item.UnitCost * item.Quantity,
                        WarehouseAreaId = string.IsNullOrWhiteSpace(item.WarehouseAreaId) ? null : item.WarehouseAreaId,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    entry.StockEntryItems.Add(newItem);

                    // Cập nhật AreaInventory nếu có WarehouseAreaId
                    if (!string.IsNullOrWhiteSpace(item.WarehouseAreaId))
                    {
                        var existingAreaInventory = await _areaInventoryRepo.GetAll(false)
                            .FirstOrDefaultAsync(ai => ai.WarehouseAreaId == item.WarehouseAreaId && ai.ProductId == item.ProductId);

                        if (existingAreaInventory != null)
                        {
                            existingAreaInventory.Quantity += item.Quantity;
                            existingAreaInventory.UpdatedBy = userId;
                            existingAreaInventory.UpdatedAt = DateTime.UtcNow;
                            _areaInventoryRepo.Update(existingAreaInventory);
                        }
                        else
                        {
                            var newAreaInventory = new AreaInventory
                            {
                                Id = Guid.NewGuid().ToString(),
                                WarehouseAreaId = item.WarehouseAreaId,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                CreatedBy = userId,
                                UpdatedBy = userId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _areaInventoryRepo.AddAsync(newAreaInventory);
                        }
                    }
                }

                entry.TotalCost = dto.Items.Sum(x => x.UnitCost * x.Quantity);
            }

            if (!string.IsNullOrWhiteSpace(dto.SupplierId))
                entry.SupplierId = dto.SupplierId;
            if (!string.IsNullOrWhiteSpace(dto.Note))
                entry.Note = dto.Note;

            entry.UpdatedBy = userId;
            entry.UpdatedAt = DateTime.UtcNow;

            _stockEntryRepo.Update(entry);
            await _stockEntryRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "StockEntries",
                entityId: entry.Id,
                description: $"Cập nhật phiếu nhập {entry.Code}",
                oldValue: null,
                newValue: new { entry.SupplierId, entry.TotalCost, entry.Note },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(entry.Id))!;
        }

        public Task<bool> DeActiveAsync(string id) => _stockEntryRepo.DeActiveAsync(id);
        public Task<bool> ActiveAsync(string id) => _stockEntryRepo.ActiveAsync(id);

        private static string GenerateCode(string prefix)
        {
            var today = DateTime.UtcNow;
            var dateStr = today.ToString("dd-MM-yyyy");
            var sequence = today.Hour * 3600 + today.Minute * 60 + today.Second; // Use hour+minute+second as sequence
            var sequenceStr = sequence.ToString("D4");
            var milliseconds = today.Millisecond.ToString("D3");
            return $"{prefix}_{sequenceStr}_{milliseconds}_{dateStr}";
        }
    }
}
