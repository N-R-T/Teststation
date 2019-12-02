using System.ComponentModel.DataAnnotations;

namespace Teststation.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(256), Display(Name = "Name")]
        public string Username { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Passwort")]
        public string Password { get; set; }
    }
}
