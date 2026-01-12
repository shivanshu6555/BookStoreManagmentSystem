using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BookStoreManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly BookStoreDBContext _context;

        public AuthController(BookStoreDBContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            User user = new();
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PAsswordHash = hashedPassword;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(UserDto request)
        {
            User user = new();
            user = await _context.Users.FirstOrDefaultAsync(a => a.Username == request.Username);
            if (user == null)
                return BadRequest("Invalid User");

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PAsswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return BadRequest("Wrong password");
            }

            return Ok("User logged in");
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetUser()
        {
            var users = _context.Users.Select(a => new UserResponseDto
            {
                Id = a.Id,
                Username = a.Username
            }).ToList();

            return Ok(users);
        }
    }
}
