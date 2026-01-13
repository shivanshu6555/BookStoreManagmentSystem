using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;

namespace BookStoreManagmentSystem.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);
    }
}
