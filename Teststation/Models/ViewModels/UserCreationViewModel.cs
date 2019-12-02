using System.ComponentModel.DataAnnotations;

namespace Teststation.Models.ViewModels
{
    public class UserCreationViewModel
    {
        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}