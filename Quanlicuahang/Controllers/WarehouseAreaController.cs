using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Warehouse;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/warehouse-area")]
    [ApiController]
    public class WarehouseAreaController : ControllerBase
    {
        private readonly IWarehouseAreaService _service;
        public WarehouseAreaController(IWarehouseAreaService service)
        {
            _service = service;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination([FromBody] WarehouseAreaSearchDto searchDto)
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
        public async Task<IActionResult> FindById([FromRoute] string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound("Khu vực kho không tồn tại");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] WarehouseAreaCreateUpdateDto dto)
        {
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
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] WarehouseAreaCreateUpdateDto dto)
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
        public async Task<IActionResult> DeActive([FromRoute] string id)
        {
            try
            {
                var result = await _service.DeActiveAsync(id);
                if (!result) return NotFound("Khu vực kho không tồn tại");
                return Ok("Ngưng hoạt động khu vực kho thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("active/{id}")]
        public async Task<IActionResult> Active([FromRoute] string id)
        {
            try
            {
                var result = await _service.ActiveAsync(id);
                if (!result) return NotFound("Khu vực kho không tồn tại");
                return Ok("Kích hoạt khu vực kho thành công");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("select_box")]
        public async Task<IActionResult> SelectBox()
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


