using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Category
    {
        [Column("CategoryId")]
        public int Id { get; set; }
        [Column("CategoryName")]
        public string Name { get; set; }
    }
}
