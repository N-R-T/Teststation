using System;
using System.Globalization;

namespace Teststation.Models
{
    public sealed class MathAnswer : Answer
    {
        public long QuestionId { get; set; }
        public MathQuestion Question { get; set; }
        public string GivenAnswer { get; set; }

        public override bool IsCorrect()
        {
            double filler = new double();
            if(double.TryParse(GivenAnswer, out filler) &&
               double.TryParse(Question.CorrectAnswer, out filler))
            {
                return Convert.ToDouble(GivenAnswer) 
                    == Convert.ToDouble(Question.CorrectAnswer);
            }
            return int.Parse(GivenAnswer, NumberStyles.HexNumber)
                == int.Parse(Question.CorrectAnswer, NumberStyles.HexNumber);
        }

        public override Question GetQuestion()
        {
            return Question;
        }
    }
}
