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
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    TotalAmount = o.TotalAmount - o.DiscountAmount,
                    DiscountAmount = o.DiscountAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    IsDeleted = o.IsDeleted
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

                var available = await _inventoryRepo.GetAvailableQuantityAsync(item.ProductId);
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
            // Minimal update: update discount and promo only for now
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            order.PromoId = string.IsNullOrWhiteSpace(dto.PromotionId) ? null : dto.PromotionId;
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
