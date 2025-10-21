using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Customer;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAllCustomers([FromBody] CustomerSearchDto searchDto)
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
        public async Task<IActionResult> Create([FromBody] CustomerCreateUpdateDto dto)
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
        public async Task<IActionResult> UpdateCategory([FromRoute] string id, [FromBody] CustomerCreateUpdateDto dto)
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
                    return NotFound("Khách hàng không tồn tại");

                return Ok("Ngưng hoạt động khách hàng thành công");
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
                    return NotFound("Khách hàng không tồn tại");

                return Ok("Kích hoạt khách hàng thành công");
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
    }
}