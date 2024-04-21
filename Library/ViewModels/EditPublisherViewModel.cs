using Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.ViewModels
{
    public class EditPublisherViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Name { get; set; }
    }
}
