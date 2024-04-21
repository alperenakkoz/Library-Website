using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [EmailAddress(ErrorMessage = "EmailValid")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }
}
