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
        public float AvgOrderValue { get; set; }
    }

    public class RevenueByEmployeeDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayment { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueByCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public float AverageOrder { get; set; }
    }

    public class RevenueByCustomerGroupDto
    {
        public string GroupName { get; set; } = string.Empty; // Ví dụ: "Hà Nội", "TP.HCM", dựa trên Address
        public decimal TotalRevenue { get; set; }
        public int CustomerCount { get; set; }
    }

    public class TopSellingProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public int StockQuantity { get; set; } // Số lượng tồn kho
    }

    public class InventoryReportDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; } // Số lượng tồn kho
        public int SoldQuantity { get; set; } // Số lượng đã bán
        public decimal UnitPrice { get; set; } // Đơn giá
        public decimal InventoryValue { get; set; } // Giá trị tồn kho (Quantity * UnitPrice)
        public string Status { get; set; } = string.Empty; // "InStock", "LowStock", "OutOfStock"
    }
}
