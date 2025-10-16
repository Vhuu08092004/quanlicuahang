namespace Quanlicuahang.DTOs.Report
{
    public class ReportDTO
    {
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
        public decimal Commission { get; set; }
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
}
