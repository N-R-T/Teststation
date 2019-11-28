using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class EvaluationViewModel
    {
        public Test Test;
        public User User;
        public List<Question> Questions;
        public List<Answer> Answers;


        public EvaluationViewModel(Test test, long userId, Database _context)
        {            
            User = _context.Users.FirstOrDefault(x => x.Id == userId);
            Test = test;
            
            Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList();
            if (Questions == null)
            {
                Questions = new List<Question>();
            }
            foreach (var question in Questions
                .Where(x => x is MultipleChoiceQuestion)
                .Select(x => x as MultipleChoiceQuestion)
                .ToList())
            {
                question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
            }

            Answers = new List<Answer>();
            var mathAnswers = _context.MathAnswers
                .Where(x => x.CandidateId == userId &&
                x.Question.TestId == test.Id)
                .ToList();
            var multipleChoiceAnswers = _context.MultipleChoiceAnswers
                .Where(x => x.CandidateId == userId &&
                x.Choice.Question.TestId == test.Id)
                .ToList();
            foreach (var answer in multipleChoiceAnswers)
            {
                answer.Choice = _context.Choices.FirstOrDefault(x => x.Id == answer.ChoiceId);
            }
            
            Answers.AddRange(mathAnswers);
            Answers.AddRange(multipleChoiceAnswers);
        }

        public EvaluationViewModel()
        {
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
                .Where(x=>x is MathAnswer)
                .Select(x=>x as MathAnswer)
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
