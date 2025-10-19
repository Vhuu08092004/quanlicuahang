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
        Task<bool> DeActiveAsync(string id);
        Task<bool> ActiveAsync(string id);
        Task<object> GetHistoryByOrderAsync(string orderId);
        Task<object> GetCashflowAsync(PaymentCashflowFilterDto filter);
        Task<string[]> GetMethodsAsync();
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IOrderRepository orderRepo,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
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
            var data = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip(skip)
                .Take(take)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !p.IsDeleted,
                    isCanDeActive = !p.IsDeleted,
                    isCanActive = p.IsDeleted
                })
                .ToListAsync();

            return new { data, total };
        }

        public async Task<PaymentDto?> GetByIdAsync(string id)
        {
            var payment = await _paymentRepo.GetAll(true)
                .Include(p => p.Order)
                .Where(p => p.Id == id)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();
            return payment;
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
            if (method != "cash" && method != "transfer")
                throw new System.Exception("Phương thức thanh toán không hợp lệ (cash | transfer)");

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
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            // Update order status
            var newPaid = paid + dto.Amount;
            if (newPaid >= netOrderAmount)
            {
                order.Status = "Completed";
                order.UpdatedBy = userId;
                order.UpdatedAt = DateTime.UtcNow;
                _orderRepo.Update(order);
                await _orderRepo.SaveChangesAsync();
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Tạo thanh toán cho đơn {order.Code} - {method} - {dto.Amount}",
                oldValue: null,
                newValue: new { payment.Id, payment.OrderId, payment.Amount, payment.PaymentMethod, payment.PaymentDate },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return (await GetByIdAsync(payment.Id))!;
        }

        public async Task<bool> DeActiveAsync(string id)
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
                if (paid < netOrderAmount)
                {
                    order.Status = "pending";
                    order.UpdatedBy = userId ?? order.UpdatedBy;
                    order.UpdatedAt = DateTime.UtcNow;
                    _orderRepo.Update(order);
                    await _orderRepo.SaveChangesAsync();
                }
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "DeActive",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Vô hiệu hóa thanh toán {payment.Id}",
                oldValue: null,
                newValue: new { payment.IsDeleted },
                userId: userId,
                ip: ip,
                userAgent: agent
            );

            return true;
        }

        public async Task<bool> ActiveAsync(string id)
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
                if (paid >= netOrderAmount)
                {
                    order.Status = "Completed";
                    order.UpdatedBy = userId ?? order.UpdatedBy;
                    order.UpdatedAt = DateTime.UtcNow;
                    _orderRepo.Update(order);
                    await _orderRepo.SaveChangesAsync();
                }
            }

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "Active",
                entityType: "Payments",
                entityId: payment.Id,
                description: $"Kích hoạt thanh toán {payment.Id}",
                oldValue: null,
                newValue: new { payment.IsDeleted },
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
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderCode = p.Order != null ? p.Order.Code : null,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
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
            return Task.FromResult(new[] { "cash", "transfer" });
        }
    }
}
