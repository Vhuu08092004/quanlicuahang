using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Category;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        // POST: api/categories/pagination
        [HttpPost("pagination")]
        public async Task<IActionResult> GetCategories([FromBody] CategorySearchDto searchDto)
        {
            var result = await _service.GetCategoriesAsync(searchDto);
            return Ok(result);
        }

        // GET: api/categories/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetCategory([FromRoute] string id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }


        // POST: api/categories/create
        [HttpPost("create")]
        public async Task<IActionResult> PostCategory([FromBody] CategoryCreateUpdateDto dto)
        {
            var result = await _service.CreateCategoryAsync(dto);
            return Ok(result);
        }

        // PUT: api/categories/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutCategory([FromRoute] string id, [FromBody] CategoryCreateUpdateDto dto)
        {
            var success = await _service.UpdateCategoryAsync(id, dto);
            if (!success) return NotFound();
            return Ok(new { message = "Cập nhật danh mục thành công." });
        }


        // POST: api/categories/deactive/{id}
        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActiveCategory([FromRoute] string id)
        {
            var success = await _service.DeActiveCategoryAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Ngưng hoạt động danh mục thành công." });
        }


        // POST: api/categories/active/{id}
        [HttpPost("active/{id}")]
        public async Task<IActionResult> ActiveCategory([FromRoute] string id)
        {
            var success = await _service.ActiveCategoryAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Kích hoạt danh mục thành công." });
        }

    }
}
