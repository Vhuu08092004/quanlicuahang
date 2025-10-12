namespace Quanlicuahang.DTOs.Category
{
    public class CategoryDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

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



    public class CategoryCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategorySelectBoxDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

}
