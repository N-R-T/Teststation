using System;
using System.Collections.Generic;
using System.Linq;

namespace Teststation.Models
{
    public class EvaluationViewModel
    {
        public Test Test;
        public User User;
        public List<Question> Questions;
        public List<Answer> Answers;
        public string UserName;

        public EvaluationViewModel(Test test, string userId, Database _context)
        {
            User = _context.Users.FirstOrDefault(x => x.Id == userId);
            Test = test;
            UserName = _context.Users.FirstOrDefault(x => x.Id == User.Id).UserName;

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
            foreach (var question in Questions
                .Where(x => x is CircuitQuestion)
                .Select(x => x as CircuitQuestion)
                .ToList())
            {
                question.Parts = _context.CircuitParts.Where(x => x.QuestionId == question.Id).ToList();
                foreach (var part in question.Parts)
                {
                    part.Resistor1 = _context.Resistors.First(x => x.Id == part.Resistor1Id);
                    part.Resistor2 = _context.Resistors.First(x => x.Id == part.Resistor2Id);
                    part.Resistor3 = _context.Resistors.First(x => x.Id == part.Resistor3Id);
                }
            }

            Answers = new List<Answer>();
            var mathAnswers = _context.MathAnswers                
                .Where(x => x.CandidateId == userId &&
                x.Question.TestId == test.Id)
                .ToList();
            foreach (var answer in mathAnswers)
            {
                answer.Question = _context.MathQuestions
                    .First(x => x.Id == answer.QuestionId);
            }

            var multipleChoiceAnswers = _context.MultipleChoiceAnswers
                .Where(x => x.CandidateId == userId &&
                x.Choice.Question.TestId == test.Id)
                .ToList();
            foreach (var answer in multipleChoiceAnswers)
            {
                answer.Choice = _context.Choices.FirstOrDefault(x => x.Id == answer.ChoiceId);
            }
            var circuitAnswers = _context.CircuitAnswers
                .Where(x => x.CandidateId == userId &&
                Questions.Where(q => q is CircuitQuestion)
                         .Select(q => q as CircuitQuestion)
                            .Any(y => y.Parts
                                .Any(z =>
                                    z.Resistor1Id == x.ResistorId ||
                                    z.Resistor2Id == x.ResistorId ||
                                    z.Resistor3Id == x.ResistorId)
                                )
                            )
                .ToList();
            foreach (var answer in circuitAnswers)
            {
                answer.Resistor = _context.Resistors.FirstOrDefault(x => x.Id == answer.ResistorId);
                answer.Resistor.CircuitPart = _context.CircuitParts.FirstOrDefault(x =>
                x.Resistor1Id == answer.ResistorId ||
                x.Resistor2Id == answer.ResistorId ||
                x.Resistor3Id == answer.ResistorId);
                answer.Resistor.CircuitPart.Question = _context.CircuitQuestions.FirstOrDefault(x => x.Id == answer.Resistor.CircuitPart.QuestionId);
            }

            Answers.AddRange(mathAnswers);
            Answers.AddRange(multipleChoiceAnswers);
            Answers.AddRange(circuitAnswers);
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
            else if (question is MultipleChoiceQuestion)
            {
                return GetPointsOfMultipleChoiceQuestion(question as MultipleChoiceQuestion);
            }
            return GetPointsOfCircuitQuestion(question as CircuitQuestion);
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
            return CalculatePointsFromChoices(question, answers);
        }

        private double GetPointsOfCircuitQuestion(CircuitQuestion question)
        {
            var answers = Answers
           .Where(x => x is CircuitAnswer)
           .Select(y => y as CircuitAnswer)
           .Where(z => z.GetQuestion().Id == question.Id)
           .ToList();

            return CalculatePointsFromResistors(question, answers);
        }

        private double CalculatePointsFromChoices(MultipleChoiceQuestion question, List<MultipleChoiceAnswer> answers)
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

        private double CalculatePointsFromResistors(CircuitQuestion question, List<CircuitAnswer> answers)
        {
            var rightUserAnswers = 0;

            foreach (var answer in answers)
            {
                if(answer.IsCorrect())
                {
                    rightUserAnswers++;
                }
            }

            double pointsPerRightChoice = (double)question.Points / (double)question.Parts.Count();
            double allPoints = (double)rightUserAnswers * (double)pointsPerRightChoice;
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
