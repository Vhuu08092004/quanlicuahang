using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IPromotionRepository
    {
        Task<List<Promotion>> GetAllPromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(string id);
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(string id);
        Task<List<Promotion>> GetActivePromotionsAsync();
    }

    public class PromotionRepository : IPromotionRepository
    {
        private readonly ApplicationDbContext _context;

        public PromotionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            return await _context.Promotions
                .Include(p => p.Orders)
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(string id)
        {
            return await _context.Promotions
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion> UpdatePromotionAsync(Promotion promotion)
        {
            _context.Entry(promotion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeletePromotionAsync(string id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Promotions
                .Include(p => p.Orders)
                .Where(p => p.StartDate <= currentDate && 
                           p.EndDate >= currentDate && 
                           p.Status == "active" &&
                           (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit))
                .ToListAsync();
        }
    }
}
