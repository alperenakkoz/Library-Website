using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class CreateRoleViewModel
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string RoleName { get; set; }
    }
}
