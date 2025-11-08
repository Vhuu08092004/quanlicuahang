namespace Quanlicuahang.DTOs.Promotion
{
    public class PromotionSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public PromotionWhereDto? Where { get; set; }
    }

    public class PromotionWhereDto
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DiscountType { get; set; }
        public decimal? MinDiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
        public int? MinUsageLimit { get; set; }
        public int? MaxUsageLimit { get; set; }
        public int? MinUsedCount { get; set; }
        public int? MaxUsedCount { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
