using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Translator
    {
        [Column("TranslatorId")]
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Column("TranslatorName")]
        public string Name { get; set; }
        public List<BookTranslator> BookTranslators { get; set; }
        [NotMapped]
        public PaginatedList<Books> PaginatedBooks { get; set; }
    }
}
