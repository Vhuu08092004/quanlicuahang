using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Product;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAll([FromBody] ProductSearchDto searchDto)
        {
            var result = await _service.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy sản phẩm");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            var result = await _service.DeActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy sản phẩm");
            return Ok("Vô hiệu hóa sản phẩm thành công");
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active(string id)
        {
            var result = await _service.ActiveAsync(id);
            if (!result) return NotFound("Không tìm thấy sản phẩm");
            return Ok("Kích hoạt sản phẩm thành công");
        }

        [HttpGet("select_box")]
        public async Task<IActionResult> GetSelectBox()
        {
            var result = await _service.GetSelectBoxAsync();
            return Ok(result);
        }
    }
}