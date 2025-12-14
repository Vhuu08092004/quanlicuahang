using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Order;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using Quanlicuahang.Enum;

namespace Quanlicuahang.Services
{
    public interface IOrderService
    {
        Task<object> GetAllAsync(OrderSearchDto searchDto);
        Task<OrderDto?> GetByIdAsync(string id);
        Task<OrderDto> CreateAsync(OrderCreateDto dto);
        Task<OrderDto> UpdateAsync(string id, OrderUpdateDto dto);
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<List<OrderDto>> GetPurchaseHistoryByCustomerAsync(string customerId);
        Task<MobileOrderResponseDto> CreateMobileOrderAsync(MobileOrderCreateDto dto);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUserRepository _userRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly IAreaInventoryRepository _areaInventoryRepo;

        public OrderService(
            IOrderRepository orderRepo,
            IProductRepository productRepo,
            IUserRepository userRepo,
            IPaymentRepository paymentRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            IAreaInventoryRepository areaInventoryRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _paymentRepo = paymentRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _areaInventoryRepo = areaInventoryRepo;
        }

        public async Task<object> GetAllAsync(OrderSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _orderRepo.GetAll(true)
                .Include(o => o.Customer)
                .Include(o => o.User)
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
                    if (System.Enum.TryParse<OrderStatus>(w.Status, true, out var status))
                    {
                        query = query.Where(o => o.Status == status);
                    }
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
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(o => new
                {
                    o.Id,
                    o.Code,
                    o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    o.PromoId,
                    o.TotalAmount,
                    o.DiscountAmount,
                    o.Status,
                    o.CreatedAt,
                    CreatedByName = o.User != null ? o.User.FullName : null,
                    o.IsDeleted,
                    o.Info,
                    o.UserId
                })
                .ToListAsync();

            var data = orders.Select(o =>
            {
                var dto = new OrderDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    PromotionId = o.PromoId,
                    TotalAmount = o.TotalAmount - o.DiscountAmount,
                    DiscountAmount = o.DiscountAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    CreatedByName = o.CreatedByName,
                    IsDeleted = o.IsDeleted,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = o.Status == OrderStatus.Pending && !o.IsDeleted,
                    isCanDeActive = !o.IsDeleted,
                    isCanActive = o.IsDeleted,
                    isCanUpdateStatus = !o.IsDeleted,
                    isCanCancel = o.Status == OrderStatus.Pending && !o.IsDeleted,
                    isCanDeliver = (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Paid) && !o.IsDeleted,
                };

                // Parse Info JSON if no Customer and no User
                if (string.IsNullOrEmpty(o.CustomerId) && string.IsNullOrEmpty(o.UserId) && !string.IsNullOrEmpty(o.Info))
                {
                    try
                    {
                        var info = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o.Info);
                        if (info != null)
                        {
                            dto.CustomerName = info.ContainsKey("CustomerName") ? info["CustomerName"] : null;
                            dto.CustomerPhone = info.ContainsKey("CustomerPhone") ? info["CustomerPhone"] : null;
                            dto.CustomerAddress = info.ContainsKey("CustomerAddress") ? info["CustomerAddress"] : null;
                            dto.Note = info.ContainsKey("Note") ? info["Note"] : null;
                        }
                    }
                    catch { }
                }

                return dto;
            }).ToList();

            return new { data, total };
        }

        public async Task<OrderDto?> GetByIdAsync(string id)
        {
            var order = await _orderRepo.GetAll(true)
                .Include(o => o.Customer)
                .Include(o => o.User)
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
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    CreatedByName = o.User != null ? (o.User.FullName ?? o.User.Username) : null,
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

            // Parse Info JSON if no Customer and no User
            if (order != null && string.IsNullOrEmpty(order.CustomerId) && string.IsNullOrEmpty(order.CreatedByName))
            {
                var orderEntity = await _orderRepo.GetByIdAsync(id);
                if (orderEntity != null && !string.IsNullOrEmpty(orderEntity.Info))
                {
                    try
                    {
                        var info = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(orderEntity.Info);
                        if (info != null)
                        {
                            order.CustomerName = info.ContainsKey("CustomerName") ? info["CustomerName"] : null;
                            order.CustomerPhone = info.ContainsKey("CustomerPhone") ? info["CustomerPhone"] : null;
                            order.CustomerAddress = info.ContainsKey("CustomerAddress") ? info["CustomerAddress"] : null;
                            order.Note = info.ContainsKey("Note") ? info["Note"] : null;
                        }
                    }
                    catch { }
                }
            }

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

                // Kiểm tra tồn kho từ AreaInventory
                var totalAvailable = await _areaInventoryRepo.GetAll(false)
                    .Where(ai => ai.ProductId == item.ProductId)
                    .SumAsync(ai => (int?)ai.Quantity) ?? 0;

                if (totalAvailable < item.Quantity)
                    throw new System.Exception($"Sản phẩm {item.ProductId} không đủ tồn kho. Còn {totalAvailable}");
            }

            // Validate payment method
            if (!EnumHelper.TryParsePaymentMethod(dto.PaymentMethod, out var paymentMethod))
            {
                throw new System.Exception($"Phương thức thanh toán không hợp lệ: {dto.PaymentMethod}. Các phương thức hỗ trợ: {string.Join(", ", System.Enum.GetNames<PaymentMethod>())}");
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

            // Gross and discount
            var gross = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
            var discount = dto.DiscountAmount;
            var netAmount = gross - discount;

            var orderDate = DateTime.UtcNow;

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Code = GenerateCode("ORDER"),
                CustomerId = string.IsNullOrWhiteSpace(dto.CustomerId) ? null : dto.CustomerId,
                UserId = validUserId,
                PromoId = string.IsNullOrWhiteSpace(dto.PromotionId) ? null : dto.PromotionId,
                OrderDate = orderDate,
                Status = netAmount > 0 ? OrderStatus.Pending : OrderStatus.Paid,
                TotalAmount = gross,
                DiscountAmount = discount,
                PaidAmount = 0,
                CreatedBy = validUserId ?? "system",
                UpdatedBy = validUserId ?? "system",
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
                    CreatedBy = validUserId ?? "system",
                    UpdatedBy = validUserId ?? "system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                order.OrderItems.Add(oi);
            }

            // Update product stock quantities (decrease by ordered quantities)
            var grouped = dto.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // Trừ số lượng từ AreaInventory (khu vực kho)
            foreach (var g in grouped)
            {
                var remainingQty = g.Qty;

                // Lấy danh sách AreaInventory có sản phẩm này, sắp xếp theo số lượng giảm dần
                var areaInventories = await _areaInventoryRepo.GetAll(false)
                    .Where(ai => ai.ProductId == g.ProductId && ai.Quantity > 0)
                    .OrderByDescending(ai => ai.Quantity)
                    .ToListAsync();

                foreach (var areaInv in areaInventories)
                {
                    if (remainingQty <= 0) break;

                    var deductQty = Math.Min(remainingQty, areaInv.Quantity);
                    areaInv.Quantity = Math.Max(0, areaInv.Quantity - deductQty);
                    areaInv.UpdatedBy = validUserId ?? "system";
                    areaInv.UpdatedAt = DateTime.UtcNow;
                    _areaInventoryRepo.Update(areaInv);

                    remainingQty -= deductQty;
                }
            }

            // Persist order + items
            await _orderRepo.SaveChangesAsync();

            // Create payment automatically if requested and amount > 0
            if (dto.CreatePayment && netAmount > 0)
            {
                // Nếu là thanh toán tiền mặt - tạo thanh toán ngay với trạng thái Completed
                if (paymentMethod == PaymentMethod.Cash)
                {
                    var payment = new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = GeneratePaymentCode(),
                        OrderId = order.Id,
                        Amount = netAmount,
                        PaymentMethod = paymentMethod,
                        PaymentStatus = PaymentStatus.Completed,
                        PaymentDate = DateTime.UtcNow,
                        IsAutoGenerated = true,
                        Note = dto.PaymentNote ?? "Thanh toán tiền mặt khi tạo đơn hàng",
                        CreatedBy = validUserId ?? "system",
                        UpdatedBy = validUserId ?? "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _paymentRepo.AddAsync(payment);

                    // Cập nhật trạng thái đơn hàng thành Paid ngay lập tức
                    order.Status = OrderStatus.Paid;
                    order.PaidAmount = netAmount;
                    order.UpdatedBy = validUserId ?? "system";
                    order.UpdatedAt = DateTime.UtcNow;

                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "Create",
                        entityType: "Payments",
                        entityId: payment.Id,
                        description: $"Tạo phiếu thanh toán tiền mặt cho đơn hàng {order.Code} - {netAmount:C}",
                        oldValue: null,
                        newValue: new { payment.Id, payment.Code, payment.OrderId, payment.Amount, payment.PaymentMethod, payment.PaymentDate, payment.PaymentStatus, payment.IsAutoGenerated },
                        userId: validUserId ?? "system",
                        ip: ip,
                        userAgent: agent
                    );
                }
                else
                {
                    // Các phương thức khác - tạo payment với trạng thái Pending, chờ xác nhận thanh toán
                    var pendingPayment = new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = GeneratePaymentCode(),
                        OrderId = order.Id,
                        Amount = netAmount,
                        PaymentMethod = paymentMethod,
                        PaymentStatus = PaymentStatus.Pending,
                        PaymentDate = DateTime.UtcNow,
                        IsAutoGenerated = true,
                        Note = dto.PaymentNote ?? $"Thanh toán {paymentMethod} - đang chờ xử lý",
                        CreatedBy = validUserId ?? "system",
                        UpdatedBy = validUserId ?? "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _paymentRepo.AddAsync(pendingPayment);

                    await _logService.LogAsync(
                        code: Guid.NewGuid().ToString(),
                        action: "CreatePendingPayment",
                        entityType: "Payments",
                        entityId: pendingPayment.Id,
                        description: $"Tạo phiếu thanh toán {paymentMethod} cho đơn hàng {order.Code} - {netAmount:C} (Pending)",
                        oldValue: null,
                        newValue: new { pendingPayment.Id, pendingPayment.Code, pendingPayment.OrderId, pendingPayment.Amount, pendingPayment.PaymentMethod, pendingPayment.PaymentStatus, pendingPayment.IsAutoGenerated },
                        userId: validUserId ?? "system",
                        ip: ip,
                        userAgent: agent
                    );
                }
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
                    PaymentMethod = paymentMethod,
                    Items = dto.Items.Select(x => new { x.ProductId, x.Quantity, x.UnitPrice }).ToList()
                },
                userId: validUserId ?? "system",
                ip: ip,
                userAgent: agent
            );

            var result = (await GetByIdAsync(order.Id))!;

            // Thêm thông tin về việc cần thanh toán QR
            if (paymentMethod != PaymentMethod.Cash && dto.CreatePayment && netAmount > 0)
            {
                result.Items = result.Items ?? new List<OrderItemDto>(); // Ensure Items is not null
                // Có thể thêm metadata về QR payment requirement vào đây nếu cần
            }

            return result;
        }

        public async Task<OrderDto> UpdateAsync(string id, OrderUpdateDto dto)
        {
            // Load existing order with items
            var order = await _orderRepo.GetAll(true)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new System.Exception("Không tìm thấy đơn hàng");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            // 1) Cập nhật trạng thái nếu có
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!System.Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
                    throw new System.Exception($"Trạng thái không hợp lệ: {dto.Status}");

                var currentStatus = order.Status;

                var allowed = IsStatusTransitionAllowed(currentStatus, newStatus);

                if (!allowed)
                    throw new System.Exception($"Chuyển trạng thái {currentStatus} -> {newStatus} không hợp lệ.");

                if (newStatus == OrderStatus.Cancelled)
                {
                    // Hoàn kho theo số lượng hiện tại của đơn
                    var groups = order.OrderItems.Where(oi => !oi.IsDeleted)
                        .GroupBy(i => i.ProductId)
                        .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                        .ToList();

                    // Hoàn lại số lượng vào AreaInventory khi hủy đơn
                    foreach (var g in groups)
                    {
                        // Tìm AreaInventory đầu tiên có sản phẩm này để cộng lại
                        var areaInventory = await _areaInventoryRepo.GetAll(false)
                            .FirstOrDefaultAsync(ai => ai.ProductId == g.ProductId);

                        if (areaInventory != null)
                        {
                            areaInventory.Quantity += Math.Abs(g.Qty);
                            areaInventory.UpdatedBy = userId;
                            areaInventory.UpdatedAt = DateTime.UtcNow;
                            _areaInventoryRepo.Update(areaInventory);
                        }
                    }
                }

                order.Status = newStatus;
                order.UpdatedBy = userId;
                order.UpdatedAt = DateTime.UtcNow;
                _orderRepo.Update(order);
                await _orderRepo.SaveChangesAsync();

                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "UpdateStatus",
                    entityType: "Orders",
                    entityId: order.Id,
                    description: $"Cập nhật trạng thái: {currentStatus} -> {newStatus}",
                    oldValue: new { Status = currentStatus },
                    newValue: new { Status = newStatus },
                    userId: userId,
                    ip: ip,
                    userAgent: agent
                );

                return (await GetByIdAsync(order.Id))!;
            }

            // 2) Chỉnh sửa nội dung đơn: chỉ khi Pending
            if (order.Status != OrderStatus.Pending)
                throw new System.Exception("Chỉ cho phép chỉnh sửa khi đơn hàng đang Pending");

            // Validate items nếu được truyền vào
            if (dto.Items != null)
            {
                if (dto.Items.Count == 0)
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
                }

                var oldGroups = order.OrderItems
                    .Where(oi => !oi.IsDeleted)
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
                var newGroups = dto.Items
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                // Điều chỉnh tồn kho theo delta
                var affectedProductIds = oldGroups.Keys.Union(newGroups.Keys).Distinct().ToList();

                foreach (var pid in affectedProductIds)
                {
                    var oldQty = oldGroups.ContainsKey(pid) ? oldGroups[pid] : 0;
                    var newQty = newGroups.ContainsKey(pid) ? newGroups[pid] : 0;
                    var delta = newQty - oldQty;

                    if (delta != 0)
                    {
                        // Kiểm tra tồn kho từ AreaInventory nếu delta > 0
                        if (delta > 0)
                        {
                            var totalAvailable = await _areaInventoryRepo.GetAll(false)
                                .Where(ai => ai.ProductId == pid)
                                .SumAsync(ai => (int?)ai.Quantity) ?? 0;

                            if (totalAvailable < delta)
                                throw new System.Exception($"Sản phẩm {pid} không đủ tồn kho. Cần thêm {delta}, còn {totalAvailable}");
                        }

                        // Trừ số lượng từ AreaInventory nếu delta > 0 (số lượng mới lớn hơn số lượng cũ)
                        if (delta > 0)
                        {
                            var remainingQty = delta;

                            // Lấy danh sách AreaInventory có sản phẩm này, sắp xếp theo số lượng giảm dần
                            var areaInventories = await _areaInventoryRepo.GetAll(false)
                                .Where(ai => ai.ProductId == pid && ai.Quantity > 0)
                                .OrderByDescending(ai => ai.Quantity)
                                .ToListAsync();

                            foreach (var areaInv in areaInventories)
                            {
                                if (remainingQty <= 0) break;

                                var deductQty = Math.Min(remainingQty, areaInv.Quantity);
                                areaInv.Quantity = Math.Max(0, areaInv.Quantity - deductQty);
                                areaInv.UpdatedBy = userId;
                                areaInv.UpdatedAt = DateTime.UtcNow;
                                _areaInventoryRepo.Update(areaInv);

                                remainingQty -= deductQty;
                            }
                        }
                        // Nếu delta < 0 (số lượng mới nhỏ hơn số lượng cũ), cần cộng lại vào AreaInventory
                        else if (delta < 0)
                        {
                            var addQty = Math.Abs(delta);

                            // Tìm AreaInventory đầu tiên có sản phẩm này để cộng lại
                            var areaInventory = await _areaInventoryRepo.GetAll(false)
                                .FirstOrDefaultAsync(ai => ai.ProductId == pid);

                            if (areaInventory != null)
                            {
                                areaInventory.Quantity += addQty;
                                areaInventory.UpdatedBy = userId;
                                areaInventory.UpdatedAt = DateTime.UtcNow;
                                _areaInventoryRepo.Update(areaInventory);
                            }
                        }
                    }
                }

                // Soft-delete toàn bộ items cũ (chỉ các items chưa bị xóa)
                foreach (var oi in order.OrderItems.Where(oi => !oi.IsDeleted))
                {
                    oi.IsDeleted = true;
                    oi.UpdatedBy = userId;
                    oi.UpdatedAt = DateTime.UtcNow;
                }

                // Thêm items mới
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

                // Cập nhật tổng tiền theo items mới
                order.TotalAmount = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
            }

            // Cập nhật các field khác nếu được truyền
            if (!string.IsNullOrWhiteSpace(dto.CustomerId))
                order.CustomerId = dto.CustomerId;
            if (!string.IsNullOrWhiteSpace(dto.PromotionId))
                order.PromoId = dto.PromotionId;
            if (dto.DiscountAmount.HasValue)
                order.DiscountAmount = dto.DiscountAmount.Value;
            if (dto.TotalAmount.HasValue && dto.Items == null)
                order.TotalAmount = dto.TotalAmount.Value;

            order.UpdatedBy = userId;
            order.UpdatedAt = DateTime.UtcNow;

            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Orders",
                entityId: order.Id,
                description: $"Cập nhật đơn hàng {order.Code}",
                oldValue: null,
                newValue: new { order.CustomerId, order.PromoId, order.TotalAmount, order.DiscountAmount },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(order.Id))!;
        }

        public Task<bool> DeActiveAsync(string id) => _orderRepo.DeActiveAsync(id);

        public Task<bool> ActiveAsync(string id) => _orderRepo.ActiveAsync(id);

        private static string GenerateCode(string prefix)
        {
            var today = DateTime.UtcNow;
            var dateStr = today.ToString("dd-MM-yyyy");
            var sequence = today.Hour * 3600 + today.Minute * 60 + today.Second; // Use hour+minute+second as sequence
            var sequenceStr = sequence.ToString("D4");
            var milliseconds = today.Millisecond.ToString("D3");
            return $"{prefix}_{sequenceStr}_{milliseconds}_{dateStr}";
        }

        private static string GeneratePaymentCode()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"pay-{timestamp}-{randomPart}";
        }

        private static bool IsStatusTransitionAllowed(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus == OrderStatus.Confirmed ||
                                     newStatus == OrderStatus.Paid ||
                                     newStatus == OrderStatus.Cancelled,

                OrderStatus.Confirmed => newStatus == OrderStatus.Paid ||
                                       newStatus == OrderStatus.Cancelled,

                OrderStatus.Paid => newStatus == OrderStatus.Cancelled,

                OrderStatus.Cancelled => false, // Không thể chuyển từ Cancelled sang trạng thái khác

                _ => false
            };
        }

        public async Task<List<OrderDto>> GetPurchaseHistoryByCustomerAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException("Mã khách hàng không hợp lệ");
            }

            var orders = await _orderRepo.GetAll(true)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    TotalAmount = o.TotalAmount - o.DiscountAmount,
                    DiscountAmount = o.DiscountAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    CreatedByName = o.User != null ? (o.User.FullName ?? o.User.Username) : null,
                    Items = o.OrderItems
                        .Where(oi => !oi.IsDeleted)
                        .Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductCode = oi.Product != null ? oi.Product.Code : null,
                            ProductName = oi.Product != null ? oi.Product.Name : null,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.Price
                        })
                        .ToList()
                })
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// API đặt hàng từ Mobile App - Tạo đơn hàng và trừ số lượng sản phẩm
        /// </summary>
        public async Task<MobileOrderResponseDto> CreateMobileOrderAsync(MobileOrderCreateDto dto)
        {
            // Validate sản phẩm tồn tại và đủ số lượng
            foreach (var item in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new System.Exception($"Sản phẩm '{item.ProductName}' không tồn tại");
                }

                if (product.IsDeleted)
                {
                    throw new System.Exception($"Sản phẩm '{product.Name}' đã bị vô hiệu hóa");
                }

                // Kiểm tra tồn kho từ AreaInventory
                var totalAvailable = await _areaInventoryRepo.GetAll(false)
                    .Where(ai => ai.ProductId == item.ProductId)
                    .SumAsync(ai => (int?)ai.Quantity) ?? 0;

                if (totalAvailable < item.Quantity)
                {
                    throw new System.Exception($"Sản phẩm '{product.Name}' chỉ còn {totalAvailable} trong kho, không đủ để bán {item.Quantity}");
                }
            }

            // Tạo mã đơn hàng tự động
            var orderCode = GenerateCode("ORDER");

            // Tạo JSON string thông tin khách hàng
            var customerInfo = new
            {
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerAddress = dto.CustomerAddress,
                Note = dto.Note
            };
            var customerInfoJson = System.Text.Json.JsonSerializer.Serialize(customerInfo);

            // Tạo Order
            var order = new Models.Order
            {
                Id = Guid.NewGuid().ToString(),
                Code = orderCode,
                CustomerId = null, // Mobile app không có customer ID
                UserId = null, // Không có user đặt hàng (khách vãng lai)
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = 0,
                PaidAmount = 0,
                Info = customerInfoJson, // Lưu thông tin khách hàng dạng JSON
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Mobile App",
                IsDeleted = false
            };

            // Tạo OrderItems và trừ số lượng sản phẩm
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Price * item.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Mobile App",
                    IsDeleted = false
                };
                orderItems.Add(orderItem);

                // Trừ số lượng từ AreaInventory (khu vực kho)
                var remainingQty = item.Quantity;

                // Lấy danh sách AreaInventory có sản phẩm này, sắp xếp theo số lượng giảm dần
                var areaInventories = await _areaInventoryRepo.GetAll(false)
                    .Where(ai => ai.ProductId == item.ProductId && ai.Quantity > 0)
                    .OrderByDescending(ai => ai.Quantity)
                    .ToListAsync();

                foreach (var areaInv in areaInventories)
                {
                    if (remainingQty <= 0) break;

                    var deductQty = Math.Min(remainingQty, areaInv.Quantity);
                    areaInv.Quantity = Math.Max(0, areaInv.Quantity - deductQty);
                    areaInv.UpdatedBy = "Mobile App";
                    areaInv.UpdatedAt = DateTime.UtcNow;
                    _areaInventoryRepo.Update(areaInv);

                    remainingQty -= deductQty;
                }
            }

            order.OrderItems = orderItems;

            // Lưu order
            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            // Tạo ghi chú về khách hàng trong payment note (dạng text thông thường)
            var paymentNote = $"Khách hàng: {dto.CustomerName}\nSĐT: {dto.CustomerPhone}\nĐịa chỉ: {dto.CustomerAddress}";
            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                paymentNote += $"\nGhi chú: {dto.Note}";
            }

            // Tạo payment record (chưa thanh toán)
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = order.Id,
                Amount = 0, // Chưa thanh toán
                PaymentMethod = PaymentMethod.Cash,
                PaymentStatus = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                Note = paymentNote,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Mobile App",
                IsDeleted = false
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            // Log action
            try
            {
                await _logService.LogAsync(
                    code: orderCode,
                    action: "CREATE_MOBILE_ORDER",
                    entityType: "Order",
                    entityId: order.Id,
                    description: $"Đơn hàng {orderCode} được tạo từ Mobile App. Khách hàng: {dto.CustomerName}, SĐT: {dto.CustomerPhone}",
                    oldValue: null,
                    newValue: null,
                    userId: null,
                    ip: null,
                    userAgent: "Mobile App"
                );
            }
            catch { /* Ignore log errors */ }

            return new MobileOrderResponseDto
            {
                OrderId = order.Id,
                OrderCode = orderCode,
                Success = true,
                Message = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.",
                OrderDate = order.OrderDate
            };
        }
    }
}