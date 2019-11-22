using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class MathAnswer : Answer
    {
        public long QuestionId { get; set; }
        public MathQuestion Question { get; set; }
        public string GivenAnswer { get; set; }

        public override bool IsCorrect()
        {
            return GivenAnswer == Question.CorrectAnswer;
        }

        public override Question GetQuestion()
        {
            return Question;
        }
    }
}
