using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Library.Models
{
    [PrimaryKey(nameof(LibraryUserId), nameof(BooksId))]
    public class Hold
    {       
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; } = DateTime.Now;
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; } = DateTime.Now.AddDays(2);
        public string LibraryUserId { get; set; }
        public int BooksId { get; set; }
        public LibraryUser LibraryUser { get; set; }
        public Books Books { get; set; }
    }
}
