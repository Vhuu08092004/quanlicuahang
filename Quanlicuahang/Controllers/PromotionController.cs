using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Promotion;
using Quanlicuahang.Models;
using Quanlicuahang.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Quanlicuahang.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Authorize]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpPost("pagination")]
        public async Task<ActionResult> GetAllPromotions([FromBody] PromotionSearchDto searchDto)
        {
            var result = await _promotionService.GetAllPromotionsAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quanlicuahang.DTOs.Promotion.PromotionListDto>> GetPromotionById(string id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
                return NotFound();

            return Ok(MapToListDto(promotion));
        }


        [HttpGet("detail/{id}")]
        public async Task<ActionResult<PromotionDetailDTO>> GetDetailPromotionById(string id)
        {
            var promotionDetail = await _promotionService.GetPromotionDetailByIdAsync(id);
            if (promotionDetail == null)
                return NotFound();

            return Ok(promotionDetail);
        }

        [HttpPost]
        public async Task<ActionResult<Quanlicuahang.DTOs.Promotion.PromotionListDto>> CreatePromotion([FromBody] Promotion promotion)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var createdPromotion = await _promotionService.CreatePromotionAsync(promotion, userId);
                return CreatedAtAction(nameof(GetPromotionById), new { id = createdPromotion.Id }, MapToListDto(createdPromotion));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Quanlicuahang.DTOs.Promotion.PromotionListDto>> UpdatePromotion(string id, [FromBody] Quanlicuahang.DTOs.Promotion.UpdatePromotionDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var updatedPromotion = await _promotionService.UpdatePromotionAsync(id, updateDto, userId);
                return Ok(MapToListDto(updatedPromotion));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePromotion(string id)
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (!result)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult<bool>> DeactivatePromotion(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var result = await _promotionService.DeactivatePromotionAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult<bool>> ActivatePromotion(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var result = await _promotionService.ActivatePromotionAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<Quanlicuahang.DTOs.Promotion.PromotionListDto>>> GetActivePromotions()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            var list = promotions.Select(p => MapToListDto(p)).ToList();
            return Ok(list);
        }

        private Quanlicuahang.DTOs.Promotion.PromotionListDto MapToListDto(Models.Promotion p)
        {
            return new Quanlicuahang.DTOs.Promotion.PromotionListDto
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
                ComputedStatus = p.GetComputedStatus(),
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
            };
        }

        [HttpGet("validate")]
        public async Task<ActionResult<bool>> ValidatePromotion([FromQuery] string code, [FromQuery] decimal orderAmount)
        {
            var isValid = await _promotionService.IsPromotionValidAsync(code, orderAmount);
            return Ok(isValid);
        }

        [HttpPost("{id}/use")]
        public async Task<ActionResult<bool>> UsePromotion(string id)
        {
            try
            {
                var result = await _promotionService.UsePromotionAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("applicable")]
        public async Task<ActionResult<List<Quanlicuahang.DTOs.Promotion.PromotionListDto>>> GetApplicablePromotions([FromQuery] decimal orderAmount)
        {
            var promotions = await _promotionService.GetApplicablePromotionsAsync(orderAmount);
            return Ok(promotions);
        }
    }
}