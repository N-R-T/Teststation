using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class EvaluationViewModel
    {
        private readonly Database _context;

        public Test Test;
        public User User;
        public List<Question> Questions;
        public List<Answer> Answers;

        public EvaluationViewModel()
        {
        }

        public EvaluationViewModel(long UserId, long TestId)
        {
            Test = _context.Tests.SingleOrDefault(x => x.Id == TestId);
        }

        public string GetGrade()
        {
            if (GetPercentage() * 100 <= Consts.neededPercentage)
            {
                return Consts.badGrade;
            }
            return Consts.goodGrade;
        }

        public string GetResult()
        {
            return GetPercentage().ToString("P");
        }

        public double GetPercentage()
        {
            return (double)GetReachedPoints() / (double)Test.GetAllPoints();
        }

        public int GetReachedPoints()
        {
            var result = 0;

            foreach (var question in Questions)
            {
                result += GetPointsOfQuestion(question);
            }

            return result;
        }

        public int GetPointsOfQuestion(Question question)
        {
            if (question is MathQuestion)
            {
                return GetPointsOfMathQuestion(question as MathQuestion);
            }
            return GetPointsOfMultipleChoiceQuestion(question as MultipleChoiceQuestion);
        }

        private int GetPointsOfMathQuestion(MathQuestion question)
        {
            if (Answers
                .SingleOrDefault(x => x.GetQuestion().Id == question.Id)
                .IsCorrect())
            {
                return question.Points;
            }

            return 0;
        }

        private int GetPointsOfMultipleChoiceQuestion(MultipleChoiceQuestion question)
        {
            var answers = Answers
                .Where(x => x is MultipleChoiceAnswer)
                .Select(y => y as MultipleChoiceAnswer)
                .Where(z => z.GetQuestion().Id == question.Id)
                .ToList();
            var rightChoices = 0;
            double pointsPerRightChoice = question.Points / question.Choices.Count;

            foreach (var choice in question.Choices)
            {
                var userChecked = answers.Any(x => x.ChoiceId == choice.Id);
                if (userChecked && choice.Correct ||
                   !userChecked && !choice.Correct)
                {
                    rightChoices++;
                }
            }
            return (int)Math.Floor(rightChoices * pointsPerRightChoice);
        }

        public static EvaluationViewModel Filler()
        {
            var frage3 = new MultipleChoiceQuestion
            {
                Id = 3,
                Position = 2,
                Text = "Was ist eine Volumen-Einheit?",
                Points = 4,
            };

            var antmögMeter = new Choice
            {
                Id = 1,
                Text = "Meter",
                Correct = false,
                Question = frage3
            };

            var antmögKubikMeter = new Choice
            {
                Id = 2,
                Text = "m³",
                Correct = true,
                Question = frage3
            };

            var antmögLiter = new Choice
            {
                Id = 3,
                Text = "Liter",
                Correct = true,
                Question = frage3
            };

            var antmögFläche = new Choice
            {
                Id = 3,
                Text = "cm²",
                Correct = false,
                Question = frage3
            };


            var frage1 = new MathQuestion
            {
                Id = 1,
                Position = 1,
                Text = "4 + 60",
                CorrectAnswer = "64",
                Points = 4,
            };

            var frage2 = new MathQuestion
            {
                Id = 2,
                Position = 3,
                Text = "Wurzel von 25",
                CorrectAnswer = "5",
                Points = 4,
            };



            frage3.Choices = new List<Choice>
{
            antmögFläche, antmögMeter, antmögLiter, antmögKubikMeter
        };

            var test = new Test
            {
                Topic = "Beispeiltest",
                Questions = new List<Question>
                        {
                            frage1, frage2, frage3
                        }
            };

            test.Questions = test.Questions.OrderBy(x => x.Position).ToList();

            var user = new User
            {
                Name = "Reiko Rücker",
                DayOfLastActivity = DateTime.Now,
                Role = UserRole.Candidate,
            };

            List<Answer> antworten = new List<Answer>
            {
            new MathAnswer
                {
                    Question = frage1,
                    GivenAnswer = "69"
                },
            new MathAnswer
                {
                    Question = frage2,
                    GivenAnswer = "5"
                },
            new MultipleChoiceAnswer
                {
                    ChoiceId = antmögMeter.Id,
                    Choice = antmögMeter
                },
            new MultipleChoiceAnswer
                {
                    ChoiceId = antmögKubikMeter.Id,
                    Choice = antmögKubikMeter
                }
            };

            return new EvaluationViewModel
            {
                Test = test,
                User = user,
                Questions = new List<Question> { frage1, frage2, frage3 },
                Answers = antworten
            };
        }
    }
}
