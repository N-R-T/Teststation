using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Teststation.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(256), Display(Name = "Name")]
        public string Username { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Passwort")]
        public string Password { get; set; }

        public List<string> UsernameErrors { get; set; }
        public List<string> PasswordErrors { get; set; }

        public RegisterViewModel()
        {
            UsernameErrors = new List<string>();
            PasswordErrors = new List<string>();
        }

        public void RegisterErrors(Database _context)
        {
            UsernameErrors = new List<string>();
            PasswordErrors = new List<string>();
            var user = _context.Users.FirstOrDefault(x => x.UserName == Username);
            if (user != null)
            {
                UsernameErrors.Add("Dieser Benutzername existiert bereits.");

                if(Password.Length < Consts.minimalPasswordLength)
                {
                    PasswordErrors.Add("Das Passwort ist nicht lang genug!");
                }
            }            
        }
    }
}
