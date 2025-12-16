using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IInvoiceSettingRepository : IBaseRepository<InvoiceSetting> { }

    public class InvoiceSettingRepository : BaseRepository<InvoiceSetting>, IInvoiceSettingRepository
    {
        public InvoiceSettingRepository(ApplicationDbContext context) : base(context) { }
    }
}

