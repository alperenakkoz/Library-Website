using Microsoft.EntityFrameworkCore;

namespace Library.Models
{
    [PrimaryKey(nameof(BooksId), nameof(TranslatorId))]
    public class BookTranslator
    {
        public int BooksId { get; set; }
        public int TranslatorId { get; set; }
        public Books Book { get; set; }
        public Translator Translator { get; set; }
    }
}
