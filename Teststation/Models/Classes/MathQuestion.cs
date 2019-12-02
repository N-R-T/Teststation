using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public class MathQuestion : Question
    {
        [Display(Name = "Korrekte Antwort")]
        public string CorrectAnswer { get; set; }
    }
}
