namespace Quanlicuahang.DTOs.Warehouse
{
    public class WarehouseAreaProductDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class WarehouseAreaDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WarehouseId { get; set; } = string.Empty;
        public string? WarehouseName { get; set; }
        public List<WarehouseAreaProductDto> Products { get; set; } = new();
    }

    public class WarehouseAreaWhereDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? WarehouseId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class WarehouseAreaSearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public WarehouseAreaWhereDto? Where { get; set; }
    }

    public class WarehouseAreaCreateUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? WarehouseId { get; set; }
    }

    public class WarehouseAreaTransferDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string FromWarehouseAreaId { get; set; } = string.Empty;
        public string ToWarehouseAreaId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}


