using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface ICategoryRepository : IBaseRepository<Category> { }

    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
    }
}
