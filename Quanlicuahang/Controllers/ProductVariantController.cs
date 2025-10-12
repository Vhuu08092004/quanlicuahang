using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.ProductVariant;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [ApiController]
    [Route("api/product_variant")]
    [Authorize]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _service;

        public ProductVariantController(IProductVariantService service)
        {
            _service = service;
        }


        [HttpPost("pagination")]
        public async Task<IActionResult> GetAllProductVariant([FromBody] ProductVariantSearchDto searchDto)
        {
            try
            {
                var result = await _service.GetAllAsync(searchDto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ind_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound("Biến thể không tồn tại");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductVariantCreateUpdateDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ProductVariantCreateUpdateDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActive(string id)
        {
            try
            {
                var result = await _service.DeActiveAsync(id);
                if (!result)
                    return NotFound("Biến thể không tồn tại");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active([FromRoute] string id)
        {
            try
            {
                var result = await _service.ActiveAsync(id);
                if (!result)
                    return NotFound("Biến thể không tồn tại");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}