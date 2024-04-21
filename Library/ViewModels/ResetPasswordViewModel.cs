using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessage = "MismatchError")]
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}
