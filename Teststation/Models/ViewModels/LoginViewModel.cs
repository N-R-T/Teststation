using System.ComponentModel.DataAnnotations;

namespace Teststation.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, Display(Name = "Name")]
        public string Username { get; set; }

        [DataType(DataType.Password), Display(Name = "Passwort")]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }

    }
}