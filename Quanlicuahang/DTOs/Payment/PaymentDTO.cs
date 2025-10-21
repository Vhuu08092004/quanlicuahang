using Quanlicuahang.DTOs;

namespace Quanlicuahang.DTOs.Payment
{
    public class PaymentDto : BaseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "cash"; // cash | transfer
        public DateTime PaymentDate { get; set; }
    }

    public class PaymentCreateDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "cash"; // cash | transfer
        public DateTime? PaymentDate { get; set; }
    }

    public class PaymentSearchDto : BaseSearchDto
    {
        public PaymentSearchWhereDto? Where { get; set; }
    }

    public class PaymentSearchWhereDto
    {
        public string? OrderCode { get; set; }
        public string? PaymentMethod { get; set; } // cash | transfer
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class PaymentCashflowFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
    }
}
