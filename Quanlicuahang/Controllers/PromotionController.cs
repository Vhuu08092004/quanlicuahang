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

        [HttpGet]
        public async Task<ActionResult<List<Promotion>>> GetAllPromotions()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Promotion>> GetPromotionById(string id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
                return NotFound();

            return Ok(promotion);
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
        public async Task<ActionResult<Promotion>> CreatePromotion([FromBody] Promotion promotion)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var createdPromotion = await _promotionService.CreatePromotionAsync(promotion, userId);
                return CreatedAtAction(nameof(GetPromotionById), new { id = createdPromotion.Id }, createdPromotion);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Promotion>> UpdatePromotion(string id, [FromBody] Promotion promotion)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var updatedPromotion = await _promotionService.UpdatePromotionAsync(id, promotion, userId);
                return Ok(updatedPromotion);
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
        public async Task<ActionResult<List<Promotion>>> GetActivePromotions()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            return Ok(promotions);
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
    }
}