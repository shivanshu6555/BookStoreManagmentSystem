using BookStoreManagmentSystem.Models;

namespace BookStoreManagmentSystem.DTO_s
{
    public class AuthorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BookResponseDto> Books { get; set; }
    }

    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public int StockQuantity { get; set; }
        public BookCategory Category { get; set; }
    }
}
