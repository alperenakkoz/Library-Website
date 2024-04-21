using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Publisher
    {
        [Column("PublisherId")]
        public int Id { get; set; }
        [Column("PublisherName")]
        public string Name { get; set; }
        public List<Books> Books { get; set; }
        [NotMapped]
        public PaginatedList<Books> PaginatedBooks { get; set; }
    }
}
