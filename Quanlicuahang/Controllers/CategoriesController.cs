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

        // POST: api/Categories/list
        [HttpPost("pagination")]
        public async Task<IActionResult> GetCategories([FromBody] CategorySearchDto searchDto)
        {
            var result = await _service.GetCategoriesAsync(searchDto);
            return Ok(result);
        }

        // POST: api/Categories/detail
        [HttpPost("detail")]
        public async Task<IActionResult> GetCategory([FromBody] CategoryIdDto dto)
        {
            var category = await _service.GetCategoryByIdAsync(dto.Id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        // POST: api/Categories/create
        [HttpPost("create")]
        public async Task<IActionResult> PostCategory([FromBody] CategoryCreateUpdateDto dto)
        {
            var result = await _service.CreateCategoryAsync(dto);
            return Ok(result);
        }

        // POST: api/Categories/update
        [HttpPost("update")]
        public async Task<IActionResult> PutCategory([FromBody] CategoryUpdateRequestDto dto)
        {
            var success = await _service.UpdateCategoryAsync(dto.Id, dto.Data);
            if (!success) return NotFound();
            return Ok(new { message = "Cập nhật danh mục thành công." });
        }

        // POST: api/Categories/delete
        [HttpPost("delete")]
        public async Task<IActionResult> SoftDeleteCategory([FromBody] CategoryIdDto dto)
        {
            var success = await _service.SoftDeleteCategoryAsync(dto.Id);
            if (!success) return NotFound();
            return Ok(new { message = "Xóa danh mục thành công." });
        }

        // POST: api/Categories/restore
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreCategory([FromBody] CategoryIdDto dto)
        {
            var success = await _service.RestoreCategoryAsync(dto.Id);
            if (!success) return NotFound();
            return Ok(new { message = "Danh mục đã được khôi phục." });
        }
    }
}
