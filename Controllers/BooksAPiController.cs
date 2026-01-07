using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookStoreManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class BooksAPiController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public BooksAPiController(BookStoreDBContext context)
        {
            _context = context;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<BookResponseDto>>> GetBooks()
        {
            var Books = await _context.Books.Include(a => a.Author).Select( a => new BookResponseDto { 
            Id = a.Id,
            Title = a.Title,
            Author = a.Author.Name,
            Price = a.Price,
            StockQuantity = a.StockQuantity,
            Category = a.Category,
            }).ToListAsync();

            if (!Books.Any())
                return NotFound();

            return Ok(Books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorResponseDto>> GetAuthorById(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound();

            return Ok(new AuthorResponseDto
            {
                Id = author.Id,
                Name = author.Name,
                Books = author.Books.Select(b => new BookResponseDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Price = b.Price,
                        StockQuantity = b.StockQuantity,
                        Category = b.Category
                    }).ToList()
            });
        }

        [HttpPost]

        //public async Task<ActionResult<Books>> PostBooks(Books books)
        //{
        //    _context.Books.Add(books);
        //    await _context.SaveChangesAsync();
        //    return Ok(await _context.Books.ToListAsync());
        //}
        public async Task<ActionResult<Books>> PostBooks(CreateAuthorDto request)
        {
            var newAuthor = new Author
            {
                Name = request.Name,
            };

            var books = request.Books.Select(b => new Books { Title = b.Title, Price = b.price, StockQuantity = b.stockq, Category = b.category, Author = newAuthor });

            newAuthor.Books = books.ToList();

            _context.Authors.Add(newAuthor);
            await _context.SaveChangesAsync();

            var response = new AuthorResponseDto
            {
                Id = newAuthor.Id,
                Name = newAuthor.Name,
                Books = newAuthor.Books.Select(b => new BookResponseDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Price = b.Price,
                    StockQuantity = b.StockQuantity,
                    Category = b.Category
                }).ToList()
            };

            return CreatedAtAction(nameof(GetAuthorById),
                new { id = response.Id },
                response);

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Books>> PutBooks(int id, Books updatedBook)
        {
            if (id != updatedBook.Id)
                return BadRequest("ID mismatch");

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound("Book not found");
            
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Price = updatedBook.Price;
            book.StockQuantity = updatedBook.StockQuantity;
            book.Category = updatedBook.Category;

            await _context.SaveChangesAsync();

            return Ok(book);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchBook(int id, [FromBody] JsonPatchDocument<Books> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            patchDoc.ApplyTo(book, ModelState);

            if(!TryValidateModel(book))
                return ValidationProblem(ModelState);

            await _context.SaveChangesAsync();

            return Ok(book);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok("deleted book id : " + id);
        }
    }
}
