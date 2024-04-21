using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class EditRoleViewModel
    {
        [Required]
        public string Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string RoleName { get; set; }
        public List<string>? Users { get; set; }
    }
}
