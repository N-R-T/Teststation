using System;

namespace Teststation.Models
{
    public class MathAnswer : Answer
    {
        public long QuestionId { get; set; }
        public MathQuestion Question { get; set; }
        public string GivenAnswer { get; set; }

        public override bool IsCorrect()
        {
            return Convert.ToDouble(GivenAnswer) == Convert.ToDouble(Question.CorrectAnswer);
        }

        public override Question GetQuestion()
        {
            return Question;
        }
    }
}
