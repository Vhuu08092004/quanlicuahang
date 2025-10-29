using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;
using Quanlicuahang.DTOs.Promotion;
using Quanlicuahang.DTOs.Order;
using Quanlicuahang.DTOs;
// using Quanlicuahang.DTOs.User; // UserDto is in Quanlicuahang.DTOs

namespace Quanlicuahang.Repositories
{
    public interface IPromotionRepository
    {
        IQueryable<Promotion> GetPromotionsQuery(bool includeDeleted = false);
        Task<Promotion?> GetPromotionByIdAsync(string id);
        Task<PromotionDetailDTO?> GetPromotionDetailByIdAsync(string id);
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(string id);
        Task<List<Promotion>> GetActivePromotionsAsync();
    }

    public class PromotionRepository : IPromotionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        public PromotionRepository(ApplicationDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public IQueryable<Promotion> GetPromotionsQuery(bool includeDeleted = false)
        {
            var query = _context.Promotions.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(p => p.IsDeleted == false);
            }
            return query;
        }

        public async Task<Promotion?> GetPromotionByIdAsync(string id)
        {
            return await _context.Promotions
                .Include(p => p.Orders)
                .Where(p => p.IsDeleted == false)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PromotionDetailDTO?> GetPromotionDetailByIdAsync(string id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.Orders)
                .Where(p => p.IsDeleted == false)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (promotion == null) return null;

            var createdByEntity = string.IsNullOrWhiteSpace(promotion.CreatedBy)
                ? null
                : await _userRepository.GetByIdWithRolesAsync(promotion.CreatedBy);
            var updatedByEntity = string.IsNullOrWhiteSpace(promotion.UpdatedBy)
                ? null
                : await _userRepository.GetByIdWithRolesAsync(promotion.UpdatedBy);

            var createdByUser = MapUser(createdByEntity);
            var updatedByUser = MapUser(updatedByEntity);

            var dto = new PromotionDetailDTO
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description ?? string.Empty,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinOrderAmount = promotion.MinOrderAmount,
                UsageLimit = promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                Status = promotion.Status,
                IsDeleted = promotion.IsDeleted,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt,
                CreatedBy = createdByUser ?? new UserDto(),
                UpdatedBy = updatedByUser ?? new UserDto(),
                Orders = promotion.Orders?.Select(o => new OrderDto { Id = o.Id, TotalAmount = o.TotalAmount }).ToList() ?? new List<OrderDto>()
            };
            return dto;
        }

        private static UserDto? MapUser(User? user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).ToList() ?? new List<string>(),
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                isCanView = true,
                isCanCreate = false,
                isCanEdit = false,
                isCanDeActive = false,
                isCanActive = false
            };
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

        // public async Task<bool> DeletePromotionAsync(string id)
        // {
        //     var promotion = await _context.Promotions.FindAsync(id);
        //     if (promotion == null)
        //         return false;

        //     _context.Promotions.Remove(promotion);
        //     await _context.SaveChangesAsync();
        //     return true;
        // }
        public async Task<bool> DeletePromotionAsync(string id){
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return false;

            promotion.IsDeleted = true; // 🔹 đánh dấu đã xóa
            _context.Promotions.Update(promotion);
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
                           (p.UsedCount < p.UsageLimit))
                .ToListAsync();
        }
    }
}
