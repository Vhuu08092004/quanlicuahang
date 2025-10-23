using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Payment;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetAll([FromBody] PaymentSearchDto searchDto)
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
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound("Không tìm thấy thanh toán");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
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
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] PaymentUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                if (!ok) return NotFound("Thanh toán không tồn tại");
                var after = await _service.GetByIdAsync(id);
                return Ok(after);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActive([FromRoute] string id, [FromBody] ChangeActiveDto? dto)
        {
            try
            {
                var result = await _service.DeActiveAsync(id, dto?.Reason);
                if (!result) return NotFound("Thanh toán không tồn tại");
                return Ok("Vô hiệu hóa thanh toán thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active([FromRoute] string id, [FromBody] ChangeActiveDto? dto)
        {
            try
            {
                var result = await _service.ActiveAsync(id, dto?.Reason);
                if (!result) return NotFound("Thanh toán không tồn tại");
                return Ok("Kích hoạt thanh toán thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("methods")]
        public async Task<IActionResult> GetMethods()
        {
            try
            {
                var result = await _service.GetMethodsAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/order/{orderId}")]
        public async Task<IActionResult> GetHistoryByOrder([FromRoute] string orderId)
        {
            try
            {
                var result = await _service.GetHistoryByOrderAsync(orderId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("cashflow")]
        public async Task<IActionResult> GetCashflow([FromBody] PaymentCashflowFilterDto filter)
        {
            try
            {
                var result = await _service.GetCashflowAsync(filter);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
