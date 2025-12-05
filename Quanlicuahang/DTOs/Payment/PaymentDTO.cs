using Quanlicuahang.DTOs;

namespace Quanlicuahang.DTOs.Payment
{
    public class PaymentDto : BaseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string? OrderCode { get; set; }
        public string? CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = Enum.PaymentMethod.Cash.ToString();
        public string PaymentStatus { get; set; } = Enum.PaymentStatus.Pending.ToString();
        public DateTime PaymentDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? OperatorName { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentCreateDto
    {
        public string? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = Enum.PaymentMethod.Cash.ToString();
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentSearchDto : BaseSearchDto
    {
        public PaymentSearchWhereDto? Where { get; set; }
    }

    public class PaymentSearchWhereDto
    {
        public string? OrderCode { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderStatus { get; set; }
    }

    public class PaymentCashflowFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
    }

    public class PaymentUpdateDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = Enum.PaymentMethod.Cash.ToString();
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }
    }

    public class ChangeActiveDto
    {
        public string? Reason { get; set; }
    }

    public class PaymentCompleteDto
    {
        public string? Note { get; set; }
    }

    public class PaymentCancelDto
    {
        public string? Reason { get; set; }
    }
}