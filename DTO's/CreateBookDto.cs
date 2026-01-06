using BookStoreManagmentSystem.Models;

namespace BookStoreManagmentSystem.DTO_s
{
    public record struct CreateBookDto(string Title,double price, int stockq, BookCategory category);
}
