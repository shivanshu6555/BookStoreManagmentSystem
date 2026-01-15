using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;
using BookStoreManagmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStoreManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly BookStoreDBContext _context;
        public AuthController(IAuthService authService, BookStoreDBContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("please login");
            }

            return user;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var token = await _authService.LoginAsync(request);
            if (token is null)
            {
                return BadRequest("invalid username or password");
            }

            return token;
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Id == id);
            _context.Remove(user);
            _context.SaveChangesAsync();
            return Ok(user + "Removed");
        }

        [HttpGet("User")]
        public async Task<ActionResult<User>> GetUser()
        {
            var users = _context.Users.Select(a => new UserResponseDto
            {
                Id = a.Id,
                Username = a.Username
            }).ToList();

            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("Admin-login")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are legged in as Admin");
        }
    }
}
