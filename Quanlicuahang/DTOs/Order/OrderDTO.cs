using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Order
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? Note { get; set; }
        public string? PromotionId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - DiscountAmount - PaidAmount;
        public bool IsFullyPaid => PaidAmount >= (TotalAmount - DiscountAmount);
        public string Status { get; set; } = Enum.OrderStatus.Pending.ToString();
        public DateTime OrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public bool IsDeleted { get; set; }

        public bool isCanView { get; set; } = true;
        public bool isCanCreate { get; set; } = true;
        public bool isCanEdit { get; set; } = true;
        public bool isCanDeActive { get; set; } = true;
        public bool isCanActive { get; set; } = false;
        public bool isCanUpdateStatus { get; set; } = true;
        public bool isCanCancel { get; set; } = true;
        public bool isCanDeliver { get; set; } = true;
        public bool isCanComplete { get; set; } = true;

        public List<OrderItemDto> Items { get; set; } = new();
        public List<PaymentInfoDto> Payments { get; set; } = new();
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

        [Required]
        public string PaymentMethod { get; set; } = Enum.PaymentMethod.Cash.ToString();
        
        public string? PaymentNote { get; set; }
        
        public bool CreatePayment { get; set; } = true; 

        public bool RequireQRPayment { get; set; } = false; 
    }

    public class OrderUpdateDto
    {
        public string? CustomerId { get; set; }
        public List<OrderItemCreateDto>? Items { get; set; }
        public string? PromotionId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PaymentInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
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