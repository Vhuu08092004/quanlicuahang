using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.ProductAttribute;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/product_attribute")]
    [ApiController]
    public class ProductAttributeController : ControllerBase
    {
        private readonly IProductAttributeService _service;

        public ProductAttributeController(IProductAttributeService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAll([FromBody] ProductAttributeSearchDto searchDto)
        {
            var result = await _service.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy thuộc tính");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductAttributeCreateUpdateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ProductAttributeCreateUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActive(string id)
        {
            var result = await _service.DeActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy thuộc tính");
            return Ok("Vô hiệu hóa thuộc tính thành công");
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active(string id)
        {
            var result = await _service.ActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy thuộc tính");
            return Ok("Kích hoạt thuộc tính thành công");
        }

        [HttpGet("select_box")]
        public async Task<IActionResult> GetSelectBox()
        {
            var result = await _service.GetSelectBoxAsync();
            return Ok(result);
        }
    }
}
