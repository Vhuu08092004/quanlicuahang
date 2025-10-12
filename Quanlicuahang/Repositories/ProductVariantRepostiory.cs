using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;

namespace Quanlicuahang.Repositories
{

    public interface IProductVariantRepository : IBaseRepository<ProductVariant> { }

    public class ProductVariantRepository : BaseRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context) { }
    }

    public interface IProductVariantAttributeValueRepository
    {
        Task AddRangeAsync(List<ProductVariantAttributeValue> entities);
        Task RemoveByVariantIdAsync(string variantId);
        Task<int> SaveChangesAsync();
    }

    public class ProductVariantAttributeValueRepository : IProductVariantAttributeValueRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ProductVariantAttributeValue> _dbSet;

        public ProductVariantAttributeValueRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<ProductVariantAttributeValue>();
        }

        public async Task AddRangeAsync(List<ProductVariantAttributeValue> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task RemoveByVariantIdAsync(string variantId)
        {
            var existing = await _dbSet
                .Where(vav => vav.ProductVariantId == variantId)
                .ToListAsync();
            _dbSet.RemoveRange(existing);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}