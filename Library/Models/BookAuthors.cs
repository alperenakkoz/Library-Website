using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;

namespace Library.Models
{
    [PrimaryKey(nameof(BooksId), nameof(AuthorId))]
    public class BookAuthors
    {
        public int BooksId { get; set; }       
        public int AuthorId { get; set; }
        public Books Book { get; set; } 
        public Author Author { get; set; } 
    }
}
