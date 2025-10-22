using Quanlicuahang.DTOs.ProductAttribute;

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
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public List<ProductAttributeValueDto> Attributes { get; set; } = new();
    }



    public class ProductCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = "pcs";
        public string? CategoryId { get; set; }
        public string? SupplierId { get; set; }
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
