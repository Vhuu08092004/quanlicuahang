using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;

namespace Quanlicuahang.Repositories
{
    public interface IProductAttributeValueRepository : IBaseRepository<ProductAttributeValue>
    {
        Task<List<ProductAttributeValue>> GetByProductIdAsync(string productId);
        Task AssignAttributesToProductAsync(string productId, Dictionary<string, object> attributes, string userId);
    }

    public class ProductAttributeValueRepository : BaseRepository<ProductAttributeValue>, IProductAttributeValueRepository
    {
        public ProductAttributeValueRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ProductAttributeValue>> GetByProductIdAsync(string productId)
        {
            return await _dbSet
                .Where(x => x.ProductId == productId && !x.IsDeleted)
                .Include(x => x.Attribute)
                .ToListAsync();
        }

        public async Task AssignAttributesToProductAsync(string productId, Dictionary<string, object> attributes, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID không được để trống", nameof(userId));

            var now = DateTime.UtcNow;

            foreach (var kv in attributes)
            {
                string attributeId = kv.Key;
                object value = kv.Value;

                var existing = await _dbSet
                    .FirstOrDefaultAsync(x => x.ProductId == productId && x.AttributeId == attributeId);

                if (existing == null)
                {
                    var pav = new ProductAttributeValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = productId,
                        AttributeId = attributeId,
                        DisplayOrder = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        IsDeleted = false
                    };

                    SetValue(pav, value);
                    await _dbSet.AddAsync(pav);
                }
                else
                {
                    SetValue(existing, value);
                    existing.UpdatedAt = now;
                    existing.UpdatedBy = userId;
                    existing.IsDeleted = false; 
                    
                    _dbSet.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
        }

        private void SetValue(ProductAttributeValue pav, object value)
        {
            pav.ValueString = null;
            pav.ValueDecimal = null;
            pav.ValueInt = null;
            pav.ValueBool = null;
            pav.ValueDate = null;

            switch (value)
            {
                case string s:
                    pav.ValueString = s;
                    break;
                case decimal d:
                    pav.ValueDecimal = d;
                    break;
                case int i:
                    pav.ValueInt = i;
                    break;
                case bool b:
                    pav.ValueBool = b;
                    break;
                case DateTime dt:
                    pav.ValueDate = dt;
                    break;
                default:
                    pav.ValueString = value.ToString();
                    break;
            }
        }
    }
}