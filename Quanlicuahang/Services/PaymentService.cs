using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Payment;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IPaymentService
    {
        Task<object> GetAllAsync(PaymentSearchDto searchDto);
        Task<PaymentDto?> GetByIdAsync(string id);
        Task<PaymentDto> CreateAsync(PaymentCreateDto dto);
        Task<bool> UpdateAsync(string id, PaymentUpdateDto dto);
        Task<bool> DeActiveAsync(string id, string? reason = null);
        Task<bool> ActiveAsync(string id, string? reason = null);
        Task<object> GetHistoryByOrderAsync(string orderId);
        Task<object> GetCashflowAsync(PaymentCashflowFilterDto filter);
        Task<string[]> GetMethodsAsync();
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IUserRepository _userRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IOrderRepository orderRepo,
            IUserRepository userRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<object> GetAllAsync(PaymentSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _paymentRepo.GetAll(true)
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .AsQueryable();

            if (searchDto.Where != null)
            {
                var w = searchDto.Where;
                if (!string.IsNullOrWhiteSpace(w.OrderCode))
                {
                    var code = w.OrderCode.Trim().ToLower();
                    query = query.Where(p => p.Order != null && p.Order.Code.ToLower().Contains(code));
                }
                if (!string.IsNullOrWhiteSpace(w.PaymentMethod))
                {
                    var method = w.PaymentMethod.Trim().ToLower();
                    query = query.Where(p => p.PaymentMethod.ToLower() == method);
                }
                if (!string.IsNullOrWhiteSpace(w.CustomerName))
                {
                    var customer = w.CustomerName.Trim().ToLower();
                    query = query.Where(p => p.Order != null && p.Order.Customer != null && p.Order.Customer.Name.ToLower().Contains(customer));
                }
                if (!string.IsNullOrWhiteSpace(w.OrderStatus))
                {
                    var st = w.OrderStatus.Trim().ToLower();
                    query = query.Where(p => p.Order != null && p.Order.Status.ToLower() == st);
                }
                if (w.FromDate.HasValue)
                {
                    var from = w.FromDate.Value.Date;
                    query = query.Where(p => p.PaymentDate >= from);
                }
                if (w.ToDate.HasValue)
                {
                    var to = w.ToDate.Value.Date.AddDays(1);
                    query = query.Where(p => p.PaymentDate < to);
                }
                if (w.IsDeleted.HasValue)
                {
                    query = query.Where(p => p.IsDeleted == w.IsDeleted);
                }
            }

            var total = await query.CountAsync();
            var list = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip(skip)
                .Take(take)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    CustomerName = p.Order != null && p.Order.Customer != null ? p.Order.Customer.Name : null,
                    p.Amount,
                    p.PaymentMethod,
                    p.PaymentDate,
                    p.IsDeleted,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.CreatedBy,
                    p.Note,
                    OrderStatus = p.Order != null ? p.Order.Status : null
                })
                .ToListAsync();

            var userIds = list.Select(x => x.CreatedBy).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var userMap = new Dictionary<string, string>();
            if (userIds.Any())
            {
                var users = await _userRepo.GetAll(true)
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Name = u.FullName ?? u.Username })
                    .ToListAsync();
                userMap = users.ToDictionary(u => u.Id, u => u.Name);
            }

            var data = list.Select(p => new PaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                OrderCode = p.OrderCode,
                CustomerName = p.CustomerName,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentDate = p.PaymentDate,
                IsDeleted = p.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                OperatorName = p.CreatedBy != null && userMap.ContainsKey(p.CreatedBy) ? userMap[p.CreatedBy] : null,
                OrderStatus = p.OrderStatus,
                Note = p.Note,
                isCanView = true,
                isCanCreate = true,
                isCanEdit = !p.IsDeleted,
                isCanDeActive = !p.IsDeleted,
                isCanActive = p.IsDeleted
            }).ToList();

            return new { data, total };
        }

        public async Task<PaymentDto?> GetByIdAsync(string id)
        {
            var raw = await _paymentRepo.GetAll(true)
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    CustomerName = p.Order != null && p.Order.Customer != null ? p.Order.Customer.Name : null,
                    p.Amount,
                    p.PaymentMethod,
                    p.PaymentDate,
                    p.IsDeleted,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.CreatedBy,
                    p.Note,
                    OrderStatus = p.Order != null ? p.Order.Status : null
                })
                .FirstOrDefaultAsync();

            if (raw == null) return null;

            string? opName = null;
            if (!string.IsNullOrEmpty(raw.CreatedBy))
            {
                var user = await _userRepo.GetByIdAsync(raw.CreatedBy);
                opName = user?.FullName ?? user?.Username;
            }

            return new PaymentDto
            {
                Id = raw.Id,
                OrderId = raw.OrderId,
                OrderCode = raw.OrderCode,
                CustomerName = raw.CustomerName,
                Amount = raw.Amount,
                PaymentMethod = raw.PaymentMethod,
                PaymentDate = raw.PaymentDate,
                IsDeleted = raw.IsDeleted,
                CreatedAt = raw.CreatedAt,
                UpdatedAt = raw.UpdatedAt,
                OperatorName = opName,
                OrderStatus = raw.OrderStatus,
                Note = raw.Note
            };
        }

        public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
        {
            if (dto.Amount <= 0)
                throw new System.Exception("Số tiền thanh toán phải lớn hơn 0");
            
            // Resolve order by Id or Code
            Order? order = null;
            if (!string.IsNullOrWhiteSpace(dto.OrderId))
            {
                order = await _orderRepo.GetByIdAsync(dto.OrderId);
            }
            else if (!string.IsNullOrWhiteSpace(dto.OrderCode))
            {
                var code = dto.OrderCode.Trim().ToLower();
                order = await _orderRepo.GetAll(false)
                    .Where(o => o.Code.ToLower() == code)
                    .FirstOrDefaultAsync();
            }
            else
            {
                throw new System.Exception("Thiếu OrderId hoặc OrderCode");
            }

            if (order == null || order.IsDeleted)
                throw new System.Exception("Đơn hàng không tồn tại hoặc đã bị xóa");

            var method = (dto.PaymentMethod ?? "cash").Trim().ToLower();
            var allowed = new[] { "cash", "transfer", "card", "momo", "zalopay" };
            if (!allowed.Contains(method))
                throw new System.Exception("Phương thức thanh toán không hợp lệ (cash | transfer | card | momo | zalopay)");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            var netOrderAmount = order.TotalAmount - order.DiscountAmount;
            var paid = await _paymentRepo.GetTotalPaidByOrderAsync(order.Id);

            if (paid + dto.Amount > netOrderAmount)
                throw new System.Exception($"Tổng thanh toán vượt quá giá trị đơn hàng còn lại. Đã thanh toán: {paid}, cần thanh toán: {netOrderAmount - paid}");

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = order.Id,
                Amount = dto.Amount,
                PaymentMethod = method,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Note = dto.Note
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            // Update order status
            var newPaid = paid + dto.Amount;
            UpdateOrderStatus(order, newPaid, netOrderAmount, userId);
            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Tạo thanh toán cho đơn {order.Code} - {method} - {dto.Amount}",
                oldValue: null,
                newValue: new { payment.Id, payment.OrderId, payment.Amount, payment.PaymentMethod, payment.PaymentDate, payment.Note },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(payment.Id))!;
        }

        public async Task<bool> UpdateAsync(string id, PaymentUpdateDto dto)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null || payment.IsDeleted) return false;

            var order = await _orderRepo.GetByIdAsync(payment.OrderId);
            if (order == null || order.IsDeleted) return false;

            if (dto.Amount <= 0) throw new System.Exception("Số tiền thanh toán phải lớn hơn 0");

            var allowed = new[] { "cash", "transfer", "card", "momo", "zalopay" };
            var method = (dto.PaymentMethod ?? payment.PaymentMethod).Trim().ToLower();
            if (!allowed.Contains(method)) throw new System.Exception("Phương thức thanh toán không hợp lệ (cash | transfer | card | momo | zalopay)");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();

            var netOrderAmount = order.TotalAmount - order.DiscountAmount;
            var paidExcluding = await _paymentRepo.GetTotalPaidByOrderExcludingAsync(order.Id, payment.Id);
            if (paidExcluding + dto.Amount > netOrderAmount)
                throw new System.Exception($"Tổng thanh toán vượt quá giá trị đơn hàng còn lại. Đã thanh toán: {paidExcluding}, cần thanh toán: {netOrderAmount - paidExcluding}");

            var before = new { payment.Amount, payment.PaymentMethod, payment.PaymentDate, payment.Note };

            payment.Amount = dto.Amount;
            payment.PaymentMethod = method;
            payment.PaymentDate = dto.PaymentDate ?? payment.PaymentDate;
            payment.Note = dto.Note;
            payment.UpdatedBy = userId ?? payment.UpdatedBy;
            payment.UpdatedAt = DateTime.UtcNow;

            _paymentRepo.Update(payment);
            await _paymentRepo.SaveChangesAsync();

            var newPaid = paidExcluding + payment.Amount;
            UpdateOrderStatus(order, newPaid, netOrderAmount, userId);
            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Cập nhật thanh toán {payment.Id}",
                oldValue: before,
                newValue: new { payment.Amount, payment.PaymentMethod, payment.PaymentDate, payment.Note },
                userId: userId,
                ip: _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                userAgent: _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString()
            );

            return true;
        }

        public async Task<bool> DeActiveAsync(string id, string? reason = null)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            payment.IsDeleted = true;
            payment.UpdatedBy = userId ?? payment.UpdatedBy;
            payment.UpdatedAt = DateTime.UtcNow;
            _paymentRepo.Update(payment);
            await _paymentRepo.SaveChangesAsync();

            // Re-evaluate order status
            var order = await _orderRepo.GetByIdAsync(payment.OrderId);
            if (order != null && !order.IsDeleted)
            {
                var netOrderAmount = order.TotalAmount - order.DiscountAmount;
                var paid = await _paymentRepo.GetTotalPaidByOrderAsync(order.Id);
                UpdateOrderStatus(order, paid, netOrderAmount, userId);
                _orderRepo.Update(order);
                await _orderRepo.SaveChangesAsync();
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Vô hiệu hóa thanh toán {payment.Id}{(string.IsNullOrWhiteSpace(reason) ? string.Empty : ": " + reason)}",
                oldValue: null,
                newValue: new { payment.IsDeleted, reason },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id, string? reason = null)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return false;

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

            payment.IsDeleted = false;
            payment.UpdatedBy = userId ?? payment.UpdatedBy;
            payment.UpdatedAt = DateTime.UtcNow;
            _paymentRepo.Update(payment);
            await _paymentRepo.SaveChangesAsync();

            // Re-evaluate order status
            var order = await _orderRepo.GetByIdAsync(payment.OrderId);
            if (order != null && !order.IsDeleted)
            {
                var netOrderAmount = order.TotalAmount - order.DiscountAmount;
                var paid = await _paymentRepo.GetTotalPaidByOrderAsync(order.Id);
                UpdateOrderStatus(order, paid, netOrderAmount, userId);
                _orderRepo.Update(order);
                await _orderRepo.SaveChangesAsync();
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Kích hoạt thanh toán {payment.Id}{(string.IsNullOrWhiteSpace(reason) ? string.Empty : ": " + reason)}",
                oldValue: null,
                newValue: new { payment.IsDeleted, reason },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<object> GetHistoryByOrderAsync(string orderId)
        {
            var query = _paymentRepo.GetAll(true)
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    CustomerName = p.Order != null && p.Order.Customer != null ? p.Order.Customer.Name : null,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Note = p.Note,
                    OrderStatus = p.Order != null ? p.Order.Status : null
                });

            var data = await query.ToListAsync();
            var total = data.Count;
            return new { data, total };
        }

        public async Task<object> GetCashflowAsync(PaymentCashflowFilterDto filter)
        {
            return await _paymentRepo.GetCashflowAsync(filter.FromDate, filter.ToDate, filter.Skip, filter.Take);
        }

        public Task<string[]> GetMethodsAsync()
        {
            return Task.FromResult(new[] { "cash", "transfer", "card", "momo", "zalopay" });
        }

        private static void UpdateOrderStatus(Order order, decimal paid, decimal netAmount, string? userId)
        {
            var newStatus = paid <= 0 ? "pending" : (paid >= netAmount ? "Completed" : "PartiallyPaid");
            order.Status = newStatus;
            order.UpdatedBy = userId ?? order.UpdatedBy;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}
