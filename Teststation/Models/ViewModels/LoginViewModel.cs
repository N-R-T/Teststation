using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public string UsernameError { get; set; }
        public string PasswordError { get; set; }

        public void LoginErrors(Database _context)
        {
            var user = _context.Users.FirstOrDefault(x=>x.UserName == Username);
            if(user == null)
            {
                UsernameError = "Dieser Benutzername existiert nicht.";
            }
            else
            {
                PasswordError = "Das Passwort ist inkorrekt.";
            }
        }
    }
}