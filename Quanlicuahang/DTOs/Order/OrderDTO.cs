using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Order
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Optional action flags for FE
        public bool isCanView { get; set; } = true;
        public bool isCanCreate { get; set; } = true;
        public bool isCanEdit { get; set; } = true;
        public bool isCanDeActive { get; set; } = true;
        public bool isCanActive { get; set; } = false;
    }

    public class OrderItemCreateDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class OrderCreateDto
    {
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        [Required]
        public List<OrderItemCreateDto> Items { get; set; } = new();
        public string? PromotionId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderWhereDto
    {
        public string? Code { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class OrderSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public OrderWhereDto? Where { get; set; }
    }
}
