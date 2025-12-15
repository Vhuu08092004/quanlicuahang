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
        private readonly ICloudinaryService _cloudinaryService;

        public ProductController(IProductService service, IWebHostEnvironment env, ICloudinaryService cloudinaryService)
        {
            _service = service;
            _env = env;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAll([FromBody] ProductSearchDto searchDto)
        {
            var result = await _service.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpPost("mobile/pagination")]
        public async Task<IActionResult> GetAllAsyncWithQuantity([FromBody] ProductSearchDto searchDto)
        {
            var result = await _service.GetAllAsyncWithQuantity(searchDto);
            return Ok(result);
        }

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy sản phẩm");
            return Ok(result);
        }

        [HttpGet("mobile/find_by_id/{id}")]
        public async Task<IActionResult> GetByIdAsyncWithQuantity(string id)
        {
            var result = await _service.GetByIdAsyncWithQuantity(id);
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
                // Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "products");
                
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