using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.Category
{
    public class CategoryDto : BaseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EntityActionDto Actions { get; set; } = new();

    }

    public class CategorySearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public CategoryWhereDto? Where { get; set; }

    }

    public class CategoryWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CategoryIdDto
    {
        public string Id { get; set; } = string.Empty;
    }

    public class CategoryCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CategoryUpdateRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public CategoryCreateUpdateDto Data { get; set; } = new();
    }

}
