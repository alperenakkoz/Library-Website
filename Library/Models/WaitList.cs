using Microsoft.EntityFrameworkCore;

namespace Library.Models
{
    [PrimaryKey(nameof(LibraryUserId), nameof(BooksId))]
    public class WaitList
    {
        public string LibraryUserId { get; set; }
        public int BooksId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public LibraryUser LibraryUser { get; set; }
        public Books Books { get; set; }
    }
}
