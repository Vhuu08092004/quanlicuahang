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
        private readonly IWebHostEnvironment _env;

        public ProductController(IProductService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
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
                // Create directory if not exists
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "assests", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the relative URL
                var imageUrl = $"/images/{uniqueFileName}";
                return Ok(new { imageUrl, fileName = uniqueFileName });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Lỗi khi upload hình ảnh: {ex.Message}");
            }
        }

        [HttpDelete("delete-image/{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "assests", "images", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok("Xóa hình ảnh thành công");
                }
                return NotFound("Không tìm thấy hình ảnh");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa hình ảnh: {ex.Message}");
            }
        }
    }
}