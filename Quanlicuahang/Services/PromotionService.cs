using Quanlicuahang.DTOs.Order;
using Quanlicuahang.DTOs.Promotion;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Quanlicuahang.Services
{
    public interface IPromotionService
    {
        Task<object> GetAllPromotionsAsync(PromotionSearchDto searchDto);
        Task<Promotion?> GetPromotionByIdAsync(string id);
        Task<PromotionDetailDTO?> GetPromotionDetailByIdAsync(string id);
        Task<Promotion> CreatePromotionAsync(Promotion promotion, string userId);
        Task<Promotion> UpdatePromotionAsync(string id, Promotion promotion, string userId);
        Task<bool> DeletePromotionAsync(string id);
        Task<bool> DeactivatePromotionAsync(string id, string userId);
        Task<bool> ActivatePromotionAsync(string id, string userId);
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

        public async Task<object> GetAllPromotionsAsync(PromotionSearchDto searchDto)
        {
            var skip = searchDto.Skip < 0 ? 0 : searchDto.Skip;
            var take = searchDto.Take <= 0 ? 10 : searchDto.Take;

            var query = _promotionRepository.GetPromotionsQuery(false);

            if (searchDto.Where != null)
            {
                var where = searchDto.Where;

                // Filter by Code
                if (!string.IsNullOrWhiteSpace(where.Code))
                {
                    var code = where.Code.Trim().ToLower();
                    query = query.Where(p => p.Code.ToLower().Contains(code));
                }

                // Filter by Description
                if (!string.IsNullOrWhiteSpace(where.Description))
                {
                    var description = where.Description.Trim().ToLower();
                    query = query.Where(p => p.Description != null && p.Description.ToLower().Contains(description));
                }

                // Filter by Discount Type
                if (!string.IsNullOrWhiteSpace(where.DiscountType))
                {
                    var discountType = where.DiscountType.Trim().ToLower();
                    query = query.Where(p => p.DiscountType.ToLower() == discountType);
                }

                // Filter by Discount Value Range
                if (where.MinDiscountValue.HasValue)
                {
                    query = query.Where(p => p.DiscountValue >= where.MinDiscountValue.Value);
                }

                if (where.MaxDiscountValue.HasValue)
                {
                    query = query.Where(p => p.DiscountValue <= where.MaxDiscountValue.Value);
                }

                // Filter by Date Range - Tìm promotion hoạt động trong khoảng thời gian
                // Promotion phải có: StartDate <= EndDateFilter VÀ EndDate >= StartDateFilter
                if (where.StartDate.HasValue)
                {
                    // Promotion phải kết thúc sau hoặc bằng ngày bắt đầu filter
                    var filterStartDate = where.StartDate.Value.Date;
                    query = query.Where(p => p.EndDate.Date >= filterStartDate);
                }

                if (where.EndDate.HasValue)
                {
                    // Promotion phải bắt đầu trước hoặc bằng ngày kết thúc filter
                    var filterEndDate = where.EndDate.Value.Date;
                    query = query.Where(p => p.StartDate.Date <= filterEndDate);
                }

                // Filter by Min Order Amount Range
                if (where.MinOrderAmount.HasValue)
                {
                    query = query.Where(p => p.MinOrderAmount >= where.MinOrderAmount.Value);
                }

                if (where.MaxOrderAmount.HasValue)
                {
                    query = query.Where(p => p.MinOrderAmount <= where.MaxOrderAmount.Value);
                }

                // Filter by Usage Limit Range
                if (where.MinUsageLimit.HasValue)
                {
                    query = query.Where(p => p.UsageLimit >= where.MinUsageLimit.Value);
                }

                if (where.MaxUsageLimit.HasValue)
                {
                    query = query.Where(p => p.UsageLimit <= where.MaxUsageLimit.Value);
                }

                // Filter by Used Count Range
                if (where.MinUsedCount.HasValue)
                {
                    query = query.Where(p => p.UsedCount >= where.MinUsedCount.Value);
                }

                if (where.MaxUsedCount.HasValue)
                {
                    query = query.Where(p => p.UsedCount <= where.MaxUsedCount.Value);
                }

                // Filter by Status
                if (!string.IsNullOrWhiteSpace(where.Status))
                {
                    var status = where.Status.Trim().ToLower();
                    query = query.Where(p => p.Status.ToLower() == status);
                }

                // Filter by IsDeleted
                if (where.IsDeleted.HasValue)
                {
                    query = query.Where(p => p.IsDeleted == where.IsDeleted.Value);
                }
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new PromotionListDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    MinOrderAmount = p.MinOrderAmount,
                    UsageLimit = p.UsageLimit,
                    UsedCount = p.UsedCount,
                    Status = p.Status,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedBy = p.UpdatedBy,
                    isCanView = true,
                    isCanCreate = true,
                    isCanEdit = !p.IsDeleted,
                    isCanDelete = !p.IsDeleted,
                    isCanActive = p.IsDeleted,
                    isCanDeActive = !p.IsDeleted
                })
                .ToListAsync();

            return new
            {
                data,
                total
            };
        }

        public async Task<PromotionDetailDTO?> GetPromotionDetailByIdAsync(string id)
        {
            return await _promotionRepository.GetPromotionDetailByIdAsync(id);
        }

        public async Task<Promotion?> GetPromotionByIdAsync(string id)
        {
            return await _promotionRepository.GetPromotionByIdAsync(id);
        }
        public async Task<List<Promotion>> GetApplicablePromotionsAsync(OrderDto order){

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var promotions = await _promotionRepository.GetPromotionsQuery(false)
                .Include(p => p.Orders)
                .ToListAsync();
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
            promotion.Id = Guid.NewGuid().ToString();
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

        public async Task<bool> DeactivatePromotionAsync(string id, string userId)
        {
            var promotion = await _promotionRepository.GetPromotionByIdAsync(id)
                ?? throw new KeyNotFoundException($"Promotion with ID {id} not found");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ValidationException("User ID is required to deactivate a promotion");

            if (promotion.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                return false;

            promotion.Status = "inactive";
            promotion.UpdatedBy = userId;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _promotionRepository.UpdatePromotionAsync(promotion);
            return true;
        }

        public async Task<bool> ActivatePromotionAsync(string id, string userId)
        {
            var promotion = await _promotionRepository.GetPromotionByIdAsync(id)
                ?? throw new KeyNotFoundException($"Promotion with ID {id} not found");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ValidationException("User ID is required to activate a promotion");

            if (promotion.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                return false;

            promotion.Status = "active";
            promotion.UpdatedBy = userId;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _promotionRepository.UpdatePromotionAsync(promotion);
            return true;
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
