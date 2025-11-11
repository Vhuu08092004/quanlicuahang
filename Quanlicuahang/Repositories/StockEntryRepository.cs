using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IStockEntryRepository : IBaseRepository<StockEntry> { }

    public class StockEntryRepository : BaseRepository<StockEntry>, IStockEntryRepository
    {
        public StockEntryRepository(ApplicationDbContext context) : base(context) { }
    }
}
