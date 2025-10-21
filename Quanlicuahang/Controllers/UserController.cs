using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("pagination")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserSearchDto searchDto)
        {
            var result = await _userService.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateUpdateDto request)
        {
            try
            {
                var user = await _userService.CreateAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserCreateUpdateDto request)
        {
            try
            {
                var result = await _userService.UpdateAsync(id, request);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("deactive/{id}")]
        public async Task<IActionResult> DeActiveUser(string id)
        {
            var result = await _userService.DeActiveAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("activate/{id}")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var result = await _userService.ActiveAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
