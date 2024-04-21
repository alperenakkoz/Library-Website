using Library.ViewModels;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Books
    {
        [Column("BookId")]
        public int Id { get; set; }
        public string Title { get; set; }

        [RegularExpression(@"^(?:\d{10})$", ErrorMessage = "The ISBN-10 must be 10 digits long.")]
        public string? ISBN10 { get; set; }

        [RegularExpression(@"^(?:\d{13})$", ErrorMessage = "The ISBN-13 must be 13 digits long.")]
        public string? ISBN13 { get; set; }
        public int NumberOfPages { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PublicationDate { get; set; }      
        public List<BookAuthors> BookAuthors { get; set; }
        [NotMapped]
        public List<Author> Authors { get; set; }
        public int BookLanguageId { get; set; }
        public BookLanguage BookLanguage { get; set; }
        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<WaitList>  WaitList { get; set; }
        public List<Rent> Rent { get; set; }
        public List<Hold> Hold { get; set; }

        public string Description { get; set; } = string.Empty;
        public List<BookTranslator> BookTranslators { get; set; }
        [NotMapped]
        public List<Translator> Translators {  get; set; } 
        public string? ImagePath { get; set; }
        public int numberOfBooks { get; set; } = 1;

        public List<Comments> Comments { get; set; }
        [NotMapped]
        public List<CommentViewModel> CommentViewModel { get; set; }
    }
}
