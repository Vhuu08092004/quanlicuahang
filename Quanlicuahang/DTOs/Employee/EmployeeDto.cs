using Humanizer;

namespace Quanlicuahang.DTOs.Employee
{
    public class EmployeeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class EmployeeSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;

        public EmployeeWhereDto? Where { get; set; }
    }

    public class EmployeeWhereDto
    {
        public string? Code { get; set; } 
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class EmployeeCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
