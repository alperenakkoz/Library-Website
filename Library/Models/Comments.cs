using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Comments
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int BooksId { get; set; }
        public Books Books { get; set; }
        public string LibraryUserId { get; set; }
        public LibraryUser LibraryUser { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; }
    }
}
