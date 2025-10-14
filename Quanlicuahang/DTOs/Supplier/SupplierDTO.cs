namespace Quanlicuahang.DTOs.Supplier
{
    public class SupplierDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

    }

    public class SupplierSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public SupplierWhereDto? Where { get; set; }

    }

    public class SupplierWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool? IsDeleted { get; set; }
    }



    public class SupplierCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class SupplierSelectBoxDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
