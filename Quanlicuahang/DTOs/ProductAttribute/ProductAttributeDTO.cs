using System.ComponentModel.DataAnnotations;
namespace Quanlicuahang.DTOs.ProductAttribute
{

    public class ProductAttributeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ProductAttributeValueDto> AttributeValues { get; set; } = new();
    }

    public class ProductAttributeCreateUpdateDto
    {
        [Required, MaxLength(255)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }



    public class ProductAttributeValueDto : BaseDto
    {
        public string AttributeId { get; set; } = string.Empty;
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class ProductAttributeValueCreateUpdateDto
    {
        [Required]
        public string AttributeId { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Value { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
    }



    public class ProductAttributeSearchDto : BaseSearchDto
    {
        public ProductAttributeSearchWhereDto? Where { get; set; }
    }

    public class ProductAttributeSearchWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
