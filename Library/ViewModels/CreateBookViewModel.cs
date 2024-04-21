using Library.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class CreateBookViewModel
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Title { get; set; }

        [Display(Name = "ISBN-10")]
        [RegularExpression(@"\b\d{9}[\dX]\b", ErrorMessage = "ISBN10Valid")]
        public string? ISBN10 { get; set; }

        [Display(Name = "ISBN-13")]
        [RegularExpression(@"^(?:\d{13})$", ErrorMessage = "ISBN13Valid")]
        public string? ISBN13 { get; set; }

        [Display(Name = "NumberOfPages")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int NumberOfPages { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "PublicationDate")]
        [Required(ErrorMessage = "The {0} field is required.")]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PublicationDate { get; set; }


        [Display(Name = "Authors")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public List<int> AuthorIds { get; set; }

        [Display(Name = "BookLanguage")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int BookLanguageId { get; set; }

        [Display(Name = "Publisher")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int PublisherId { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int CategoryId { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Description { get; set; } = string.Empty;


        [Display(Name = "Translators")]
        public List<int>? TranslatorIds { get; set; }

        [Display(Name = "Image")]
        public IFormFile? Image { get; set; }

        [Display(Name = "Images")]
        public List<IFormFile>? Images { get; set; }

        [Display(Name = "NumberOfBooks")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int numberOfBooks { get; set; }
    }
}
