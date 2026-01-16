using BookStoreManagmentSystem.Models;

namespace BookStoreManagmentSystem.DTO_s
{
    public record struct AuthorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BookResponseDto> Books { get; set; }
    }

    public record struct BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }
        public BookCategory Category { get; set; }
    }

    public record struct UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }

    public record struct PaginationDto
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class PagedBooksResponseDto
    {
        public PaginationDto Pagination { get; set; }
        public List<BookResponseDto> Data { get; set; }
    }
}
