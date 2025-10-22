using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Order;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IOrderService
    {
        Task<object> GetAllAsync(OrderSearchDto searchDto);
        Task<OrderDto?> GetByIdAsync(string id);
        Task<OrderDto> CreateAsync(OrderCreateDto dto);
        Task<bool> UpdateAsync(string id, OrderCreateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public OrderService(
            IOrderRepository orderRepo,
            IInventoryRepository inventoryRepo,
            IProductRepository productRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _orderRepo = orderRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(OrderSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _orderRepo.GetAll(true)
                .Include(o => o.Customer)
                .AsQueryable();

            if (searchDto.Where != null)
            {
                var w = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(w.Code))
                {
                    var code = w.Code.Trim().ToLower();
                    query = query.Where(o => o.Code.ToLower().Contains(code));
                }
                if (!string.IsNullOrWhiteSpace(w.CustomerName))
                {
                    var cname = w.CustomerName.Trim().ToLower();
                    query = query.Where(o => o.Customer != null && o.Customer.Name.ToLower().Contains(cname));
                }
                if (!string.IsNullOrWhiteSpace(w.Status))
                {
                    var st = w.Status.Trim().ToLower();
                    query = query.Where(o => o.Status.ToLower() == st);
                }
                if (w.FromDate.HasValue)
                {
                    var from = w.FromDate.Value.Date;
                    query = query.Where(o => o.OrderDate >= from);
                }
                if (w.ToDate.HasValue)
                {
                    var to = w.ToDate.Value.Date.AddDays(1);
                    query = query.Where(o => o.OrderDate < to);
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
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    TotalAmount = o.TotalAmount - o.DiscountAmount, // FE expects net
                    DiscountAmount = o.DiscountAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    IsDeleted = o.IsDeleted,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !o.IsDeleted,
                    isCanDeActive = !o.IsDeleted,
                    isCanActive = o.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<OrderDto?> GetByIdAsync(string id)
        {
            var order = await _orderRepo.GetAll(true)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    PromotionId = o.PromoId,
                    TotalAmount = o.TotalAmount - o.DiscountAmount,
                    DiscountAmount = o.DiscountAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    IsDeleted = o.IsDeleted,
                    Items = o.OrderItems
                    .Where(oi => !oi.IsDeleted)
                    .Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductCode = oi.Product != null ? oi.Product.Code : null,
                        ProductName = oi.Product != null ? oi.Product.Name : null,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            return order;
        }

        public async Task<OrderDto> CreateAsync(OrderCreateDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new System.Exception("Đơn hàng phải có ít nhất một sản phẩm");

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new System.Exception("Số lượng phải > 0");
                if (item.UnitPrice < 0)
                    throw new System.Exception("Đơn giá không hợp lệ");

                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.IsDeleted)
                    throw new System.Exception($"Sản phẩm {item.ProductId} không tồn tại hoặc đã bị xóa");
                var available = product.Quantity;
                if (available < item.Quantity)
                    throw new System.Exception($"Sản phẩm {item.ProductId} không đủ tồn kho. Còn {available}");
            }

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
            {
                // Allow creation without token for testing or public endpoints
                userId = "system";
            }

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            // Gross and discount
            var gross = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
            var discount = dto.DiscountAmount;

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Code = GenerateCode("ORDER"),
                CustomerId = string.IsNullOrWhiteSpace(dto.CustomerId) ? null : dto.CustomerId,
                PromoId = string.IsNullOrWhiteSpace(dto.PromotionId) ? null : dto.PromotionId,
                OrderDate = DateTime.UtcNow,
                Status = "pending",
                TotalAmount = gross,
                DiscountAmount = discount,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _orderRepo.AddAsync(order);

            // Create items
            foreach (var item in dto.Items)
            {
                var oi = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice,
                    Subtotal = item.UnitPrice * item.Quantity,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                // Use DbContext via repository
                // No direct repository for OrderItem; Use orderRepo context via tracking
                // Workaround: Use EF context from orderRepo through _productRepo context? Not exposed. So attach through productRepo's context.
                // Instead: Add to order navigation
                order.OrderItems.Add(oi);
            }

            // Update product stock quantities (decrease by ordered quantities)
            var grouped = dto.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();
            foreach (var g in grouped)
            {
                var prod = await _productRepo.GetByIdAsync(g.ProductId);
                if (prod != null)
                {
                    prod.Quantity = Math.Max(0, prod.Quantity - g.Qty);
                    prod.UpdatedBy = userId;
                    prod.UpdatedAt = DateTime.UtcNow;
                    _productRepo.Update(prod);
                }
            }

            // Persist order + items
            await _orderRepo.SaveChangesAsync();

            // Create stock-out inventory records to adjust available quantity
            foreach (var item in dto.Items)
            {
                var stockOut = new Inventory
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = GenerateCode("INVOUT"),
                    ProductId = item.ProductId,
                    Quantity = -Math.Abs(item.Quantity),
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _inventoryRepo.AddAsync(stockOut);
            }

            await _orderRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Orders",
                entityId: order.Id,
                description: $"Tạo đơn hàng {order.Code}",
                oldValue: null,
                newValue: new
                {
                    order.Id,
                    order.Code,
                    order.TotalAmount,
                    order.DiscountAmount,
                    Items = dto.Items.Select(x => new { x.ProductId, x.Quantity, x.UnitPrice }).ToList()
                },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(order.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, OrderCreateDto dto)
        {
            // Load existing order with items
            var order = await _orderRepo.GetAll(true)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return false;

            if (dto.Items == null || dto.Items.Count == 0)
                throw new System.Exception("Đơn hàng phải có ít nhất một sản phẩm");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            // Validate new items and compute grouped quantities
            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new System.Exception("Số lượng phải > 0");
                if (item.UnitPrice < 0)
                    throw new System.Exception("Đơn giá không hợp lệ");
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.IsDeleted)
                    throw new System.Exception($"Sản phẩm {item.ProductId} không tồn tại hoặc đã bị xóa");
            }

            var oldGroups = order.OrderItems
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
            var newGroups = dto.Items
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Adjust stock based on delta quantities
            var affectedProductIds = oldGroups.Keys.Union(newGroups.Keys).Distinct().ToList();
            foreach (var pid in affectedProductIds)
            {
                var oldQty = oldGroups.ContainsKey(pid) ? oldGroups[pid] : 0;
                var newQty = newGroups.ContainsKey(pid) ? newGroups[pid] : 0;
                var delta = newQty - oldQty; // positive means more stock to deduct

                if (delta != 0)
                {
                    var product = await _productRepo.GetByIdAsync(pid);
                    if (product == null)
                        throw new System.Exception($"Sản phẩm {pid} không tồn tại");

                    if (delta > 0 && product.Quantity < delta)
                        throw new System.Exception($"Sản phẩm {pid} không đủ tồn kho. Cần thêm {delta}, còn {product.Quantity}");

                    product.Quantity = product.Quantity - delta; // delta>0 deduct, delta<0 add back
                    product.UpdatedBy = userId;
                    product.UpdatedAt = DateTime.UtcNow;
                    _productRepo.Update(product);

                    // Add inventory adjustment record
                    var inv = new Inventory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = GenerateCode("INVADJ"),
                        ProductId = pid,
                        Quantity = -delta, // positive stock out when delta>0
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _inventoryRepo.AddAsync(inv);
                }
            }

            // Soft-delete old items
            foreach (var oi in order.OrderItems)
            {
                oi.IsDeleted = true;
                oi.UpdatedBy = userId;
                oi.UpdatedAt = DateTime.UtcNow;
            }

            // Add new items
            foreach (var item in dto.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice,
                    Subtotal = item.UnitPrice * item.Quantity,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Update order totals and fields
            order.CustomerId = string.IsNullOrWhiteSpace(dto.CustomerId) ? null : dto.CustomerId;
            order.PromoId = string.IsNullOrWhiteSpace(dto.PromotionId) ? null : dto.PromotionId;
            order.TotalAmount = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
            order.DiscountAmount = dto.DiscountAmount;
            order.UpdatedBy = userId;
            order.UpdatedAt = DateTime.UtcNow;

            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();
            return true;
        }

        public Task<bool> DeActiveAsync(string id) => _orderRepo.DeActiveAsync(id);

        public Task<bool> ActiveAsync(string id) => _orderRepo.ActiveAsync(id);

        private static string GenerateCode(string prefix)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"{prefix.ToLower()}-{timestamp}-{randomPart}";
        }
    }
}
