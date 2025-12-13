using Quanlicuahang.DTOs;

namespace Quanlicuahang.DTOs.Promotion
{
    public class PromotionListDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ComputedStatus { get; set; } = string.Empty;
    }
}
