namespace Quanlicuahang.DTOs.Report
{
    public class ReportDTO
    {
    }

    // Pagination base class for reports
    public class ReportPaginationDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
    }

    // DTOs cho các báo cáo (định nghĩa đơn giản, bạn có thể mở rộng)
    public class RevenueReportDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueByEmployeeDto
    {
        public string EmployeeName { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayment { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueByCustomerDto
    {
        public string CustomerName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueByCustomerGroupDto
    {
        public string GroupName { get; set; } // Ví dụ: "Hà Nội", "TP.HCM", dựa trên Address
        public decimal TotalRevenue { get; set; }
        public int CustomerCount { get; set; }
    }

    public class TopSellingProductDto
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class InventoryReportDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string CategoryName { get; set; }
    }

    // Input DTOs with pagination
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
