using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStoreManagmentSystem.Services
{
    public class AuthService(BookStoreDBContext _context, IConfiguration _configuration) : IAuthService
    {
        public async Task<string?> LoginAsync(UserDto request)
        {
           
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Username == request.Username);
            if (user == null)
                return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PAsswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }


            return CreateToken(user);
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await _context.Users.AnyAsync(a => a.Username == request.Username))
            {
                return null;
            }
            User user = new();
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PAsswordHash = hashedPassword;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken
                (
                issuer: _configuration.GetValue<string>("AppSettings:issuer"),
                audience: _configuration.GetValue<string>("AppSettings:audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
