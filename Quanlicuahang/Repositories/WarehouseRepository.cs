using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IWarehouseRepository : IBaseRepository<Warehouse> { }
    public interface IWarehouseAreaRepository : IBaseRepository<WarehouseArea> { }
    public interface IAreaInventoryRepository : IBaseRepository<AreaInventory> { }

    public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext context) : base(context) { }
    }

    public class WarehouseAreaRepository : BaseRepository<WarehouseArea>, IWarehouseAreaRepository
    {
        public WarehouseAreaRepository(ApplicationDbContext context) : base(context) { }
    }

    public class AreaInventoryRepository : BaseRepository<AreaInventory>, IAreaInventoryRepository
    {
        public AreaInventoryRepository(ApplicationDbContext context) : base(context) { }
    }
}


