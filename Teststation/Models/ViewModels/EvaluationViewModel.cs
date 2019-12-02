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
                .Where(x => x is MathAnswer)
                .Select(x => x as MathAnswer)
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
            double pointsPerRightChoice = (double)question.Points / (double)question.Choices.Count;

            foreach (var choice in question.Choices)
            {
                var userChecked = answers.Any(x => x.ChoiceId == choice.Id);
                if (userChecked && choice.Correct ||
                   !userChecked && !choice.Correct)
                {
                    rightChoices++;
                }
            }
            return (int)Math.Ceiling(rightChoices * pointsPerRightChoice);
        }
    }
}
