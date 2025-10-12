using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.ActionLog;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/action_log")]
    [ApiController]
    public class ActionLogsController : ControllerBase
    {
        private readonly IActionLogService _logService;

        public ActionLogsController(IActionLogService logService)
        {
            _logService = logService;
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> GetLogs([FromBody] ActionLogSearchDto searchDto)
        {
            var logs = await _logService.GetLogsAsync(searchDto);
            return Ok(logs);
        }
    }
}
