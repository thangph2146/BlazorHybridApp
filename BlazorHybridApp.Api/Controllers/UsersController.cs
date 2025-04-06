using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHybridApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // Don't return password in response
            user.PasswordHash = null;
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            
            // Don't return password in response
            user.PasswordHash = null;
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            // Check if email already exists
            var existingUser = await _userService.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already registered");
            }

            var createdUser = await _userService.CreateUserAsync(user);
            
            // Don't return password in response
            createdUser.PasswordHash = null;
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var updatedUser = await _userService.UpdateUserAsync(user);
            
            // Don't return password in response
            updatedUser.PasswordHash = null;
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
        {
            var isAuthenticated = await _userService.AuthenticateUserAsync(request.Email, request.Password);
            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByEmailAsync(request.Email);
            
            // Don't return password in response
            user.PasswordHash = null;
            return Ok(new { user });
        }
    }

    public class AuthRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 