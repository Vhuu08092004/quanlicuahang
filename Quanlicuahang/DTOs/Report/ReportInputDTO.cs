namespace Quanlicuahang.DTOs.Report
{
    public class DayRevenueInputDto : ReportPaginationDto
    {
        public DateTime Date { get; set; }
    }

    public class MonthRevenueInputDto : ReportPaginationDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class YearRevenueInputDto : ReportPaginationDto
    {
        public int Year { get; set; }
    }

    public class RevenueFilterDto : ReportPaginationDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Region { get; set; } // "hà nội", "hồ chí minh", "đà nẵng", hoặc null để lấy tất cả
    }

    public class TopProductsFilterDto : ReportPaginationDto
    {
        public int TopN { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class InventoryFilterDto : ReportPaginationDto
    {
    }
}
