using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Category;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }


        [HttpPost("pagination")]
        public async Task<IActionResult> GetAllCategories([FromBody] CategorySearchDto searchDto)
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

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound("Danh mục không tồn tại");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateUpdateDto dto)
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


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] string id, [FromBody] CategoryCreateUpdateDto dto)
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
        public async Task<IActionResult> DeActiveCategory([FromRoute] string id)
        {
            try
            {
                var result = await _service.DeActiveAsync(id);
                if (!result)
                    return NotFound("Sản phẩm không tồn tại");

                return Ok("Ngưng hoạt động sản phẩm thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> ActiveCategory([FromRoute] string id)
        {
            try
            {
                var result = await _service.ActiveAsync(id);
                if (!result)
                    return NotFound("Danh mục không tồn tại");

                return Ok("Kích hoạt danh mục thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("select_box")]
        public async Task<IActionResult> GetSelectBox()
        {
            try
            {
                var result = await _service.GetSelectBoxActiveAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
