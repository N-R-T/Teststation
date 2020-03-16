using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public sealed class MathQuestion : Question
    {
        [Display(Name = "Korrekte Antwort")]
        public string CorrectAnswer { get; set; }
    }
}
