using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IWarehouseAreaRepository : IBaseRepository<WarehouseArea> { }
    public interface IAreaInventoryRepository : IBaseRepository<AreaInventory>
    {
        public Task<List<AreaInventory>> GetByAreaInventoryByProductIds(string[] productIds);

    }

    public class WarehouseAreaRepository : BaseRepository<WarehouseArea>, IWarehouseAreaRepository
    {
        public WarehouseAreaRepository(ApplicationDbContext context) : base(context) { }
    }

    public class AreaInventoryRepository : BaseRepository<AreaInventory>, IAreaInventoryRepository
    {
        public AreaInventoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<AreaInventory>> GetByAreaInventoryByProductIds(string[] productIds)
        {

            return await _dbSet
                .Where(ai => productIds.Contains(ai.ProductId))
                .ToListAsync();
        }
    }
}


