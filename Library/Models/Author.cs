using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Author
    {
        [Column("AuthorId")]
        public int Id { get; set; }

        [Column("AuthorName")]
        public string Name { get; set; }
        public List<BookAuthors> BookAuthors { get; set;}       
        [NotMapped]
        public PaginatedList<Books> PaginatedBooks { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
