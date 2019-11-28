using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class MathQuestion : Question
    {
        [Display(Name = "Korrekte Antwort")]
        public string CorrectAnswer { get; set; }
    }
}
