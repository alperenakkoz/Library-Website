using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class UserInfoViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [EmailAddress(ErrorMessage = "EmailValid")]
        [Display(Name = "Email")]
        [Remote(action: "IsEmailAvailable", controller: "Account")]
        public string Email { get; set; }
    }
}
