using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IInventoryRepository : IBaseRepository<Inventory>
    {
        Task<int> GetAvailableQuantityAsync(string productId);
    }

    public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> GetAvailableQuantityAsync(string productId)
        {
            var total = await _dbSet
                .Where(i => !i.IsDeleted && i.ProductId == productId)
                .SumAsync(i => (int?)i.Quantity) ?? 0;
            return total;
        }
    }
}
