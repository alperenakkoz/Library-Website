using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class BookLanguage
    {
        [Column("LanguageId")]
        public int Id { get; set; }
        [Column("LanguageName")]
        public string Name { get; set; }
        public List<Books> Books { get; set; }
    }
}
