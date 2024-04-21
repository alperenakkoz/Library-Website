using Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.ViewModels
{
    public class EditAuthorViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        [Display(Name = "NewImage")]
        public IFormFile? NewImage { get; set; }
    }
}
