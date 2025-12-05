using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Payment;
using Quanlicuahang.Enum;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System.Text.Json;

namespace Quanlicuahang.Services
{
    public interface IPaymentQRService
    {
        Task<PaymentQRResponseDto> CreateQRPaymentAsync(PaymentQRCreateDto dto);
        Task<PaymentStatusCheckDto> CheckPaymentStatusAsync(string paymentId);
        Task<bool> VerifyPaymentAsync(PaymentVerificationDto dto);
        Task<bool> SimulatePaymentSuccessAsync(string paymentId);
    }

    public class PaymentQRService : IPaymentQRService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentService _paymentService;
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;

        public PaymentQRService(
            IPaymentRepository paymentRepo,
            IOrderRepository orderRepo,
            IPaymentService paymentService,
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _paymentService = paymentService;
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
        }

        public async Task<PaymentQRResponseDto> CreateQRPaymentAsync(PaymentQRCreateDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId);
            if (order == null || order.IsDeleted)
                throw new System.Exception("Đơn hàng không tồn tại hoặc đã bị xóa");

            if (!System.Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
                throw new System.Exception($"Phương thức thanh toán không hợp lệ: {dto.PaymentMethod}");

            if (paymentMethod == PaymentMethod.Cash)
                throw new System.Exception("Không thể tạo QR code cho thanh toán tiền mặt");

            var userId = await _tokenHelper.GetUserIdFromTokenAsync();

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                Code = GeneratePaymentCode(),
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                TransactionRef = GenerateTransactionRef(paymentMethod),
                Note = dto.Note ?? $"Thanh toán QR {paymentMethod}",
                CreatedBy = userId ?? "system",
                UpdatedBy = userId ?? "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            var qrCodeUrl = GenerateQRCodeUrl(payment, dto.Amount, paymentMethod);
            var paymentUrl = GeneratePaymentUrl(payment, paymentMethod);
            var expiryTime = DateTime.UtcNow.AddMinutes(15); // QR expires in 15 minutes

            await _logService.LogAsync(
                code: Guid.NewGuid().ToString(),
                action: "CreateQR",
                entityType: "PaymentQR",
                entityId: payment.Id,
                description: $"Tạo QR code thanh toán {paymentMethod} cho đơn hàng {order.Code}",
                oldValue: null,
                newValue: new { payment.Id, payment.TransactionRef, dto.Amount, paymentMethod },
                userId: userId ?? "system",
                ip: _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                userAgent: _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString()
            );

            return new PaymentQRResponseDto
            {
                PaymentId = payment.Id,
                OrderId = dto.OrderId,
                QRCodeUrl = qrCodeUrl,
                PaymentUrl = paymentUrl,
                Amount = dto.Amount,
                PaymentMethod = paymentMethod.ToString(),
                ExpiryTime = expiryTime,
                TransactionRef = payment.TransactionRef ?? "",
                Status = PaymentStatus.Pending.ToString()
            };
        }

        public async Task<PaymentStatusCheckDto> CheckPaymentStatusAsync(string paymentId)
        {
            var payment = await _paymentRepo.GetAll(false)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new System.Exception("Không tìm thấy giao dịch thanh toán");

            return new PaymentStatusCheckDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Status = payment.PaymentStatus.ToString(),
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                PaymentDate = payment.PaymentStatus == PaymentStatus.Completed ? payment.PaymentDate : null,
                TransactionRef = payment.TransactionRef,
                IsCompleted = payment.PaymentStatus == PaymentStatus.Completed,
                Message = GetStatusMessage(payment.PaymentStatus)
            };
        }

        public async Task<bool> VerifyPaymentAsync(PaymentVerificationDto dto)
        {
            var payment = await _paymentRepo.GetByIdAsync(dto.PaymentId);
            if (payment == null)
                return false;

            if (payment.PaymentStatus == PaymentStatus.Completed)
                return true; 

            var isVerified = await MockVerifyWithPaymentGateway(payment, dto.TransactionRef, dto.ExternalTransactionId);

            if (isVerified)
            {
                payment.PaymentStatus = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.UtcNow;
                payment.TransactionRef = dto.TransactionRef ?? payment.TransactionRef;
                payment.UpdatedAt = DateTime.UtcNow;

                _paymentRepo.Update(payment);
                await _paymentRepo.SaveChangesAsync();

                await _paymentService.UpdateOrderStatusBasedOnPaymentAsync(payment.OrderId);

                var userId = await _tokenHelper.GetUserIdFromTokenAsync();
                await _logService.LogAsync(
                    code: Guid.NewGuid().ToString(),
                    action: "VerifyPayment",
                    entityType: "PaymentQR",
                    entityId: payment.Id,
                    description: $"Xác minh thành công thanh toán QR {payment.PaymentMethod}",
                    oldValue: new { Status = PaymentStatus.Pending },
                    newValue: new { Status = PaymentStatus.Completed, payment.TransactionRef },
                    userId: userId ?? "system",
                    ip: _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    userAgent: _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString()
                );
            }

            return isVerified;
        }

        public async Task<bool> SimulatePaymentSuccessAsync(string paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null || payment.PaymentStatus == PaymentStatus.Completed)
                return false;

            payment.PaymentStatus = PaymentStatus.Completed;
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionRef = $"SIMULATED_{DateTime.UtcNow.Ticks}";
            payment.UpdatedAt = DateTime.UtcNow;

            _paymentRepo.Update(payment);
            await _paymentRepo.SaveChangesAsync();

            await _paymentService.UpdateOrderStatusBasedOnPaymentAsync(payment.OrderId);

            return true;
        }

        private static string GeneratePaymentCode()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"qrpay-{timestamp}-{randomPart}";
        }

        private static string GenerateTransactionRef(PaymentMethod paymentMethod)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var prefix = paymentMethod switch
            {
                PaymentMethod.Momo => "MOMO",
                PaymentMethod.VnPay => "VNPAY",
                PaymentMethod.Zalo => "ZALO",
                PaymentMethod.BankTransfer => "BANK",
                PaymentMethod.Card => "CARD",
                _ => "QR"
            };
            return $"{prefix}_{timestamp}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private static string GenerateQRCodeUrl(Payment payment, decimal amount, PaymentMethod paymentMethod)
        {
            var qrData = JsonSerializer.Serialize(new
            {
                PaymentId = payment.Id,
                TransactionRef = payment.TransactionRef,
                Amount = amount,
                PaymentMethod = paymentMethod.ToString(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(qrData));
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(encodedData)}";
        }

        private static string GeneratePaymentUrl(Payment payment, PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Momo => $"https://test-payment.momo.vn/v2/gateway/pay?partnerCode=TEST&orderId={payment.TransactionRef}&amount={payment.Amount}",
                PaymentMethod.VnPay => $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_TxnRef={payment.TransactionRef}&vnp_Amount={payment.Amount * 100}",
                PaymentMethod.Zalo => $"https://sb-openapi.zalopay.vn/v2/create?app_trans_id={payment.TransactionRef}&amount={payment.Amount}",
                _ => $"https://example-payment.com/pay?ref={payment.TransactionRef}&amount={payment.Amount}"
            };
        }

        private static async Task<bool> MockVerifyWithPaymentGateway(Payment payment, string? transactionRef, string? externalTransactionId)
        {
            await Task.Delay(500); 
            return true; 
        }

        private static string GetStatusMessage(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Đang chờ thanh toán. Vui lòng quét mã QR để thanh toán.",
                PaymentStatus.Completed => "Thanh toán thành công!",
                PaymentStatus.Failed => "Thanh toán thất bại. Vui lòng thử lại.",
                PaymentStatus.Cancelled => "Thanh toán đã bị hủy.",
                _ => "Trạng thái không xác định"
            };
        }
    }
}