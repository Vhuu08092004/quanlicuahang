using Quanlicuahang.DTOs.Order;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.Services
{
    public interface IPromotionService
    {
        Task<List<Promotion>> GetAllPromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(string id);
        Task<Promotion> CreatePromotionAsync(Promotion promotion, string userId);
        Task<Promotion> UpdatePromotionAsync(string id, Promotion promotion, string userId);
        Task<bool> DeletePromotionAsync(string id);
        Task<List<Promotion>> GetActivePromotionsAsync();
        Task<bool> IsPromotionValidAsync(string code, decimal orderAmount);
        Task<bool> UsePromotionAsync(string id);
        Task<List<Promotion>> GetApplicablePromotionsAsync(OrderDto order);
    }

    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;

        public PromotionService(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            return await _promotionRepository.GetAllPromotionsAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(string id)
        {
            return await _promotionRepository.GetPromotionByIdAsync(id);
        }
        public async Task<List<Promotion>> GetApplicablePromotionsAsync(OrderDto order){

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var promotions = await _promotionRepository.GetAllPromotionsAsync();
            var now = DateTime.UtcNow;

            var applicablePromotions = promotions
                .Where(p =>
                    p.Status.Equals("active", StringComparison.OrdinalIgnoreCase) &&
                    p.StartDate <= now &&
                    p.EndDate >= now &&
                    (p.UsedCount < p.UsageLimit) &&
                    order.TotalAmount >= p.MinOrderAmount
                )
                .ToList();

            return applicablePromotions;
        }


        public async Task<Promotion> CreatePromotionAsync(Promotion promotion, string userId)
        {
            ValidatePromotion(promotion);
            if(userId == null)
                throw new ValidationException("User ID is required to create a promotion");

            promotion.CreatedBy = userId;
            promotion.UpdatedBy = userId;

            return await _promotionRepository.CreatePromotionAsync(promotion);
        }

        public async Task<Promotion> UpdatePromotionAsync(string id, Promotion promotion, string userId)
        {
            var existingPromotion = await _promotionRepository.GetPromotionByIdAsync(id)
                ?? throw new KeyNotFoundException($"Promotion with ID {id} not found");

            ValidatePromotion(promotion);

            // Update properties
            existingPromotion.Code = promotion.Code;
            existingPromotion.Description = promotion.Description;
            existingPromotion.DiscountType = promotion.DiscountType;
            existingPromotion.DiscountValue = promotion.DiscountValue;
            existingPromotion.StartDate = promotion.StartDate;
            existingPromotion.EndDate = promotion.EndDate;
            existingPromotion.MinOrderAmount = promotion.MinOrderAmount;
            existingPromotion.UsageLimit = promotion.UsageLimit;
            existingPromotion.Status = promotion.Status;
            existingPromotion.UpdatedBy = userId;
            existingPromotion.UpdatedAt = DateTime.UtcNow;

            return await _promotionRepository.UpdatePromotionAsync(existingPromotion);
        }

        public async Task<bool> DeletePromotionAsync(string id)
        {
            return await _promotionRepository.DeletePromotionAsync(id);
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            return await _promotionRepository.GetActivePromotionsAsync();
        }

        public async Task<bool> IsPromotionValidAsync(string code, decimal orderAmount)
        {
            var promotions = await GetActivePromotionsAsync();
            var promotion = promotions.FirstOrDefault(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (promotion == null)
                return false;

            // Check minimum order amount
            if (orderAmount < promotion.MinOrderAmount)
                return false;

            // Check usage limit
            if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
                return false;

            return true;
        }

        public async Task<bool> UsePromotionAsync(string id)
        {
            var promotion = await _promotionRepository.GetPromotionByIdAsync(id)
                ?? throw new KeyNotFoundException($"Promotion with ID {id} not found");

            if (promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
                return false;

            promotion.UsedCount++;
            await _promotionRepository.UpdatePromotionAsync(promotion);
            return true;
        }

        private void ValidatePromotion(Promotion promotion)
        {
            if (string.IsNullOrWhiteSpace(promotion.Code))
                throw new ValidationException("Promotion code is required");

            if (promotion.DiscountValue <= 0)
                throw new ValidationException("Discount value must be greater than 0");

            if (promotion.EndDate <= promotion.StartDate)
                throw new ValidationException("End date must be after start date");

            if (promotion.MinOrderAmount < 0)
                throw new ValidationException("Minimum order amount cannot be negative");

            if (promotion.UsageLimit < 0)
                throw new ValidationException("Usage limit cannot be negative");

            if (!IsValidDiscountType(promotion.DiscountType))
                throw new ValidationException("Invalid discount type. Must be 'percent' or 'fixed'");

            if (promotion.DiscountType == "percent" && promotion.DiscountValue > 100)
                throw new ValidationException("Percentage discount cannot be greater than 100%");

            if (!IsValidStatus(promotion.Status))
                throw new ValidationException("Invalid status. Must be 'active' or 'inactive'");
        }

        private bool IsValidDiscountType(string discountType)
        {
            return discountType.ToLower() is "percent" or "fixed";
        }

        private bool IsValidStatus(string status)
        {
            return status.ToLower() is "active" or "inactive";
        }
    }
}
