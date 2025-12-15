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
        private readonly ICloudinaryService _cloudinaryService;

        public CategoryController(ICategoryService service, ICloudinaryService cloudinaryService)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
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
                var result = await _service.GetSelectBoxAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file hình ảnh");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Chỉ chấp nhận file hình ảnh (jpg, jpeg, png, gif, webp)");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Kích thước file không được vượt quá 5MB");

            try
            {
                // Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "categories");
                
                // Extract public ID from URL for later deletion if needed
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');
                var publicId = string.Join("/", segments.Skip(segments.Length - 2).Take(2)).Replace(".jpg", "").Replace(".png", "").Replace(".jpeg", "").Replace(".gif", "").Replace(".webp", "");

                return Ok(new { imageUrl, publicId });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Lỗi khi upload hình ảnh: {ex.Message}");
            }
        }

        [HttpDelete("delete-image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest("Public ID không hợp lệ");

            try
            {
                var result = await _cloudinaryService.DeleteImageAsync(publicId);
                if (result)
                    return Ok("Xóa hình ảnh thành công");
                return NotFound("Không tìm thấy hình ảnh hoặc đã bị xóa");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa hình ảnh: {ex.Message}");
            }
        }

    }
}
