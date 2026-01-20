using BookStoreManagmentSystem.Caching;
using BookStoreManagmentSystem.DTO_s;
using BookStoreManagmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using NuGet.Versioning;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BookStoreManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class BooksAPiController : ControllerBase
    {
        private readonly BookStoreDBContext _context;
        private readonly IDistributedCache _cache;      

        public BooksAPiController(BookStoreDBContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<PagedBooksResponseDto>> GetBooksFromDB(int page = 1, int pageSize = 10)
        {
            if (page <= 0 && pageSize <= 0)
            {
                return BadRequest("Invalid pagination params");
            }

            string cacheKey = $"books_page_{page}_size_{pageSize}";
            var TotalCount = _context.Books.Count();
            var TotalPages = (int)Math.Ceiling((decimal)TotalCount / pageSize);

            var CachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(CachedData))
            {
                var CachedBooks = JsonSerializer.Deserialize<PagedBooksResponseDto>(CachedData);
                return Ok(CachedBooks);
            }

            //var Books = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            //    entry.SlidingExpiration = TimeSpan.FromSeconds(60);
            //    entry.Priority = CacheItemPriority.Normal;

            //    return await _context.Books.AsNoTracking().Include(a => a.Author).OrderBy(a => a.Id).Skip((page - 1)*pageSize).Take(pageSize).Select(a => new BookResponseDto
            //    {
            //        Id = a.Id,
            //        Title = a.Title,
            //        Author = a.Author.Name,
            //        Price = a.Price,
            //        StockQuantity = a.StockQuantity,
            //        Category = a.Category,
            //    }).ToListAsync();
            //});

            var Books = await _context.Books.AsNoTracking().OrderBy(a => a.Id).Skip((page - 1) * pageSize).Take(pageSize).Include(a => a.Author).Select(a => new BookResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Author = a.Author.Name,
                Price = a.Price,
                StockQuantity = a.StockQuantity,
                Category = a.Category,
            }).ToListAsync();

            if (!Books.Any())
                return NotFound();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60),
                SlidingExpiration = TimeSpan.FromSeconds(60)
            };

            var response = new PagedBooksResponseDto()
            {
                Pagination = new PaginationDto()
                {
                    TotalPages = TotalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                Data = Books
            };
            var SerializedData = JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, SerializedData, cacheOptions);


            return Ok(response);
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
                        Author = author.Name,
                        StockQuantity = b.StockQuantity,
                        Category = b.Category
                    }).ToList()
            });
        }

        [HttpGet("GetAuthors")]
        public async Task<ActionResult<AuthorResponseDto>> GetAuthors()
        {
            var Authors = _context.Authors.AsNoTracking().Include(a => a.Books).Select(a => new AuthorResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Books = a.Books.Select(a => new BookResponseDto { Title = a.Title, Price = a.Price, Category = a.Category, Author = a.Author.Name }).ToList()
            });

            if (Authors == null)
                return NotFound();

            return Ok(Authors);
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

        [Authorize]
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
