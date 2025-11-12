using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.DTOs.StockEntry
{
    public class StockEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalCost { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public bool IsDeleted { get; set; }

        public bool isCanView { get; set; } = true;
        public bool isCanCreate { get; set; } = true;
        public bool isCanEdit { get; set; } = true;
        public bool isCanDeActive { get; set; } = true;
        public bool isCanActive { get; set; } = false;
        public bool isCanUpdateStatus { get; set; } = true;
        public bool isCanCancel { get; set; } = true;
        public bool isCanComplete { get; set; } = true;

        public List<StockEntryItemDto> Items { get; set; } = new();
    }

    public class StockEntryItemCreateDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UnitCost { get; set; }
        public string? WarehouseAreaId { get; set; }
    }

    public class StockEntryCreateDto
    {
        public string? SupplierId { get; set; }
        [Required]
        public List<StockEntryItemCreateDto> Items { get; set; } = new();
        public string? Note { get; set; }
    }

    public class StockEntryUpdateDto
    {
        public string? SupplierId { get; set; }
        public List<StockEntryItemCreateDto>? Items { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }

    public class StockEntryItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? WarehouseAreaId { get; set; }
        public string? WarehouseAreaName { get; set; }
    }

    public class StockEntryWhereDto
    {
        public string? Code { get; set; }
        public string? SupplierName { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class StockEntrySearchDto
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
        public StockEntryWhereDto? Where { get; set; }
    }
}
