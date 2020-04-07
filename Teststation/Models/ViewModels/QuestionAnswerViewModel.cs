using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public class QuestionAnswerViewModel
    {
        public long Id { get; set; }

        [Display(Name = "Fragetext")]
        public string Text { get; set; }
        public int Position { get; set; }

        [Display(Name = "Punktzahl")]
        public int Points { get; set; }
        public long TestId { get; set; }
        public List<ChoiceAnswerViewModel> Choices { get; set; }
        public List<CircuitPartAnswerViewModel> CircuitParts { get; set; }
        public CircuitType CircuitType { get; set; }
        public double Amperage { get; set; }
        public double InitialCurrent { get; set; }
        public QuestionType Type { get; set; }
        public string GivenAnswer { get; set; }
        
    }
}
