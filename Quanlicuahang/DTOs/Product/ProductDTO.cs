using Quanlicuahang.DTOs.ProductAttribute;
using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Product
{
    public class ProductDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = "pcs";
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public List<ProductAttributeValueDto> Attributes { get; set; } = new();
    }



    public class ProductCreateUpdateDto
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Mã sản phẩm không được vượt quá 50 ký tự")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Mã vạch không được vượt quá 100 ký tự")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự")]
        public string Unit { get; set; } = "pcs";

        public string? CategoryId { get; set; }
        public string? SupplierId { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductAttributeValueCreateUpdateDto> Attributes { get; set; } = new();
    }


    public class ProductSearchDto : BaseSearchDto
    {
        public ProductSearchWhereDto? Where { get; set; }
    }

    public class ProductSearchWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public string? SupplierId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
