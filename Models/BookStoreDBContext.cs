using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookStoreManagmentSystem.Models
{
    public class BookStoreDBContext : DbContext
    {
        public DbSet<Books> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Books>().Property(e => e.Category).HasConversion<string>();
            base.OnModelCreating(modelBuilder);
        }

        public BookStoreDBContext(DbContextOptions<BookStoreDBContext> options) : base(options)
        {

        }
    }
}
