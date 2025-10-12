using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.ProductVariant
{
    public class ProductVariantDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal? PriceAdjustment { get; set; }
        public int StockQuantity { get; set; }
        public List<VariantAttributeDto> Attributes { get; set; } = new();
    }

    public class ProductVariantCreateUpdateDto
    {
        [Required, MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; }

        public decimal? PriceAdjustment { get; set; } = 0;

        public int StockQuantity { get; set; } = 0;

        public List<string> AttributeValueIds { get; set; } = new();
    }

    public class VariantAttributeDto
    {
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;
        public string AttributeValueId { get; set; } = string.Empty;
    }

    public class ProductVariantSearchDto : BaseSearchDto
    {
        public ProductVariantSearchWhereDto? Where { get; set; }
    }

    public class ProductVariantSearchWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ProductId { get; set; }
        public bool? InStock { get; set; }
    }
}