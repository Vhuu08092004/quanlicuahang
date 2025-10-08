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

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories(
            string? search = null,
            int page = 1,
            int pageSize = 10,
            bool includeDeleted = false)
        {
            var result = await _service.GetCategoriesAsync(search, page, pageSize, includeDeleted);
            return Ok(result);
        }

        // GET: api/Categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(string id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<IActionResult> PostCategory(CategoryCreateUpdateDto dto)
        {
            var result = await _service.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
        }

        // PUT: api/Categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(string id, CategoryCreateUpdateDto dto)
        {
            var success = await _service.UpdateCategoryAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE: api/Categories/{id} (Soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteCategory(string id)
        {
            var success = await _service.SoftDeleteCategoryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // PUT: api/Categories/restore/{id}
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreCategory(string id)
        {
            var success = await _service.RestoreCategoryAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Danh mục đã được khôi phục." });
        }
    }
}
