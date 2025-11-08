using Quanlicuahang.DTOs.Order;
using Quanlicuahang.Services;

namespace Quanlicuahang.DTOs.Promotion
{
    // public class PromotionDTO : BaseDto
    // {
    //     public string Code { get; set; } = string.Empty;
    //     public string Name { get; set; } = string.Empty;
    //     public string? Barcode { get; set; }
    //     public decimal Price { get; set; }
    //     public string Unit { get; set; } = "pcs";
    //     public int Quantity { get; set; }
    //     public string? CategoryId { get; set; }
    //     public string? CategoryName { get; set; }
    //     public string? SupplierId { get; set; }
    //     public string? SupplierName { get; set; }
    //     public List<ProductAttributeValueDto> Attributes { get; set; } = new();
    // }



    public class PromotionDetailDTO
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDto CreatedBy { get; set; }
        public UserDto UpdatedBy { get; set; }

        public List<OrderDto> Orders { get; set; }
    }

}
