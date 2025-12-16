using Quanlicuahang.DTOs;

namespace Quanlicuahang.DTOs.Invoice
{
    public class InvoiceSettingDto : BaseDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
