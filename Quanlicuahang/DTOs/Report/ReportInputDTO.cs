namespace Quanlicuahang.DTOs.Report
{
    // Input DTOs cho các báo cáo
    public class DayRevenueInputDto
    {
        public DateTime Date { get; set; }
    }

    public class MonthRevenueInputDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class YearRevenueInputDto
    {
        public int Year { get; set; }
    }

    public class RevenueFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class TopProductsFilterDto
    {
        public int TopN { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
