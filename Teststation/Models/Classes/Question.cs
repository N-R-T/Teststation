using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public abstract class Question
    {
        public long Id { get; set; }

        [Display(Name = "Fragetext")]
        public string Text { get; set; }
        public int Position { get; set; }

        [Display(Name = "Punktzahl")]
        public int Points { get; set; }
        public long TestId { get; set; }
        public Test Test { get; set; }
    }
}
