using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public class QuestionCreationViewModel
    {
        public long Id { get; set; }

        [Display(Name = "Fragetext")]
        public string Text { get; set; }
        public int Position { get; set; }

        [Display(Name = "Punktzahl")]
        public int Points { get; set; }
        public long TestId { get; set; }
        public Test Test { get; set; }
        public string CorrectAnswer { get; set; }
        public List<Choice> Choices { get; set; }
        public string Type { get; set; }
    }
}
