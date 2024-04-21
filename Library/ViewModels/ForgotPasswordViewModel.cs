using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class ForgotPasswordViewModel
    {

        [Required(ErrorMessage = "The {0} field is required.")]
        [EmailAddress(ErrorMessage = "EmailValid")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
