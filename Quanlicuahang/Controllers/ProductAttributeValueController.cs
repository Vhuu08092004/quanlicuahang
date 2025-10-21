using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/product_attribute_value")]
    [ApiController]
    public class ProductAttributeValueController : ControllerBase
    {
        private readonly IProductAttributeValueService _service;

        public ProductAttributeValueController(IProductAttributeValueService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAll([FromBody] ProductAttributeValueSearchDto searchDto)
        {
            var result = await _service.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy giá trị thuộc tính");
            return Ok(result);
        }

        [HttpGet("find_product_by_id/{productId}")]
        public async Task<IActionResult> GetByProductId(string productId)
        {
            var result = await _service.GetByProductIdAsync(productId);
            return Ok(result);
        }

        [HttpGet("find_attribute_by_id/{attributeId}")]
        public async Task<IActionResult> GetByAttributeId(string attributeId)
        {
            var result = await _service.GetByAttributeIdAsync(attributeId);
            return Ok(result);
        }

        [HttpPost("create/product/{productId}")]
        public async Task<IActionResult> Create(string productId, [FromBody] ProductAttributeValueCreateUpdateDto dto)
        {
            var result = await _service.CreateAsync(productId, dto);
            return Ok(result);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ProductAttributeValueCreateUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok("Cập nhật giá trị thuộc tính thành công");
        }

        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActive(string id)
        {
            var result = await _service.DeActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy giá trị thuộc tính");
            return Ok("Vô hiệu hóa giá trị thuộc tính thành công");
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active(string id)
        {
            var result = await _service.ActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy giá trị thuộc tính");
            return Ok("Kích hoạt giá trị thuộc tính thành công");
        }
    }
}
