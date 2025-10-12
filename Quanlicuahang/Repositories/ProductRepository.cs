using Quanlicuahang.Data;

namespace Quanlicuahang.Repositories
{
    public interface IProductRepository : IBaseRepository<Product> { }

    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }
    }
}