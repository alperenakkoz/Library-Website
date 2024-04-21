using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword")]
        [Compare("NewPassword", ErrorMessage = "MismatchError")]
        public string ConfirmPassword { get; set; }
    }
}
