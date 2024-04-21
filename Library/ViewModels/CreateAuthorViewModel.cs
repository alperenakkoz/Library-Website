using Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.ViewModels
{
    public class CreateAuthorViewModel
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string AuthorName { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Image")]
        public IFormFile? Image { get; set; }
    }
}
