using Quanlicuahang.Enum;
using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Payment
{
    public class PaymentQRCreateDto
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = string.Empty; 
        
        public string? Note { get; set; }
    }

    public class PaymentQRResponseDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string QRCodeUrl { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public string Status { get; set; } = PaymentStatus.Pending.ToString();
    }

    public class PaymentVerificationDto
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;
        
        public string? TransactionRef { get; set; }
        
        public string? ExternalTransactionId { get; set; }
    }

    public class PaymentStatusCheckDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string? TransactionRef { get; set; }
        public bool IsCompleted { get; set; }
        public string? Message { get; set; }
    }
}