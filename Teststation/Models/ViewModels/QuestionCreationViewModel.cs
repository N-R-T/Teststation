using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class QuestionCreationViewModel
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public long TestId { get; set; }
        public Test Test { get; set; }
        public string CorrectAnswer { get; set; }
        public List<Choice> Choices { get; set; }
        public string Type { get; set; }
    }
}
