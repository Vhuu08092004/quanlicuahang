using Quanlicuahang.Data;

namespace Quanlicuahang.Repositories
{

    public interface IProductAttributeRepository : IBaseRepository<ProductAttribute> { }

    public class ProductAttributeRepository : BaseRepository<ProductAttribute>, IProductAttributeRepository
    {
        public ProductAttributeRepository(ApplicationDbContext context) : base(context) { }
    }
}