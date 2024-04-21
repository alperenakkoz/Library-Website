using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.ViewModels;
using Library.Models;

namespace Library.Models
{
    public class AppDbContext : IdentityDbContext<LibraryUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }
        public DbSet<Books> Books { get; set; } //name = tableName
        public DbSet<Author>  Author { get; set; }
        public DbSet<BookAuthors> BookAuthors { get; set; }
        public DbSet<BookTranslator> BookTranslator { get; set; }
        public DbSet<BookLanguage> BookLanguage { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Library.Models.Publisher> Publisher { get; set; } = default!;
        public DbSet<Library.Models.Translator> Translator { get; set; } = default!;
        public DbSet<WaitList> WaitList { get; set; }
        public DbSet<Rent> Rent { get; set; }
        public DbSet<Hold> Hold { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Comments> Comments { get; set; }
    }
}
