using System;
using System.Collections.Generic;
using System.Linq;

namespace Teststation.Models
{
    public class EvaluationViewModel
    {
        public Test Test;
        public UserInformation User;
        public List<Question> Questions;
        public List<Answer> Answers;
        public string UserName;

        public EvaluationViewModel(Test test, long userId, Database _context)
        {
            User = _context.UserInformation.FirstOrDefault(x => x.Id == userId);
            Test = test;
            UserName = _context.Users.FirstOrDefault(x => x.Id == User.UserId).UserName;

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

        public double GetReachedPoints()
        {
            double result = 0;

            foreach (var question in Questions)
            {
                result += GetPointsOfQuestion(question);
            }

            return result;
        }

        public double GetPointsOfQuestion(Question question)
        {
            if (question is MathQuestion)
            {
                return GetPointsOfMathQuestion(question as MathQuestion);
            }
            return GetPointsOfMultipleChoiceQuestion(question as MultipleChoiceQuestion);
        }

        private double GetPointsOfMathQuestion(MathQuestion question)
        {
            if (Answers
                .Where(x => x is MathAnswer)
                .Select(x => x as MathAnswer)
                .SingleOrDefault(x => x.GetQuestion().Id == question.Id)
                .IsCorrect())
            {
                return question.Points;
            }

            return 0;
        }

        private double GetPointsOfMultipleChoiceQuestion(MultipleChoiceQuestion question)
        {
            var answers = Answers
           .Where(x => x is MultipleChoiceAnswer)
           .Select(y => y as MultipleChoiceAnswer)
           .Where(z => z.GetQuestion().Id == question.Id)
           .ToList();
            return CalculatePointsExactly(question, answers);       
        }

        private double CalculatePointsExactly(MultipleChoiceQuestion question, List<MultipleChoiceAnswer> answers)
        {
            var rightUserChoices = 0;
            var correctChoices = question.Choices.Where(x => x.Correct);
            foreach (var choice in question.Choices)
            {
                var userChecked = answers.Any(x => x.ChoiceId == choice.Id);
                if (userChecked && choice.Correct)
                {
                    rightUserChoices++;
                }
                else if (userChecked && !choice.Correct)
                {
                    rightUserChoices--;
                }
            }
            double pointsPerRightChoice = (double)question.Points / (double)correctChoices.Count();
            double allPoints = (double)rightUserChoices * (double)pointsPerRightChoice;
            double wholePoints = Math.Floor(allPoints);
            double commaPortion = allPoints - wholePoints;
            double points = wholePoints;

            if (commaPortion > 0 && commaPortion <= 0.5)
            {
                points += 0.5;
            }
            else if (commaPortion > 0.5 && commaPortion <= 1.0)
            {
                points += 1.0;
            }

            if (points > question.Points)
            {
                return question.Points;
            }
            if (points < 0)
            {
                return 0;
            }
            return points;
        }
}
}
