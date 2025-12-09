using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Order
{
    /// <summary>
    /// DTO đơn giản cho mobile app khi đặt hàng
    /// </summary>
    public class MobileOrderCreateDto
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Danh sách sản phẩm không được để trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        public List<MobileOrderItemDto> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }

    public class MobileOrderItemDto
    {
        [Required(ErrorMessage = "ProductId không được để trống")]
        public string ProductId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class MobileOrderResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
