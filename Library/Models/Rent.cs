using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Rent
    {
        [Column("RentId")]
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; } = DateTime.Now;
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; }
        public string LibraryUserId { get; set; }
        public int BooksId { get; set; }
        public bool IsReturned { get; set; } = false;
        public LibraryUser LibraryUser { get; set; }
        public Books Books { get; set; }
    }
}
