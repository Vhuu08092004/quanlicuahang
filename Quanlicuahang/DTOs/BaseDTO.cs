namespace Quanlicuahang.DTOs
{
    public class BaseDto
    {
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EntityActionDto
    {
        public bool isCanCreate { get; set; }
        public bool isCanEdit { get; set; }
        public bool isCanDelete { get; set; }
        public bool isCanActive { get; set; }
        public bool isCanDeActive{ get; set; }
        public bool isCanView { get; set; }
        public bool isCanAssign { get; set; }
    }

    public class StatusDto
    {
         public  string? statusCode { get; set; }
         public  string? statusName { get; set; }
         public  string? statusColor{ get; set; }
         public  string? statusColorText { get; set; }

    }
}
