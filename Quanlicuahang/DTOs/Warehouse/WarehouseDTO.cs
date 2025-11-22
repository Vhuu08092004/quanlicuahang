namespace Quanlicuahang.DTOs.Warehouse
{
    public class WarehouseDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class WarehouseWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class WarehouseSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public WarehouseWhereDto? Where { get; set; }
    }

    public class WarehouseCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}


