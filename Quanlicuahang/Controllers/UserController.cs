using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs;


namespace Quanlicuahang.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly Services.UserService _userService;

        public UserController(Services.UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<UserResponse> CreateUser([FromBody] UserRequest request)
        {
            return await _userService.AddAsync(request);
        }

        [HttpPut("{id}")]
        public async Task<UserResponse?> UpdateUser(string id, [FromBody] UserUpdateRequest request)
        {
            return await _userService.UpdateAsync(id, request);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}
