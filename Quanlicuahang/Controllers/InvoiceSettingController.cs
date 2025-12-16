using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Invoice;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/invoice-setting")]
    [ApiController]
    public class InvoiceSettingController : ControllerBase
    {
        private readonly IInvoiceSettingService _service;

        public InvoiceSettingController(IInvoiceSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _service.GetAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Upsert([FromBody] InvoiceSettingUpsertDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpsertAsync(dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
