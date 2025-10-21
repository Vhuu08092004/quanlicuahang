using Humanizer;

namespace Quanlicuahang.DTOs.Customer
{
    public class CustomerDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class CustomerSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;

        public CustomerWhereDto? Where { get; set; }
    }

    public class CustomerWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CustomerCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}