namespace Quanlicuahang.DTOs
{
    public class BaseDto
    {
        public string Id { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CreatedBy { get; set; }

        public bool isCanCreate { get; set; }
        public bool isCanEdit { get; set; }
        public bool isCanDelete { get; set; }
        public bool isCanActive { get; set; }
        public bool isCanDeActive { get; set; }
        public bool isCanView { get; set; }
        public bool isCanAssign { get; set; }
    }

    public class BaseSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;

    }

    public class StatusDto
    {
        public string? statusCode { get; set; }
        public string? statusName { get; set; }
        public string? statusColor { get; set; }
        public string? statusColorText { get; set; }

    }

    public class SelectBoxDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DataType { get; set; }
    }
}
