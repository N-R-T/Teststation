using System.Collections.Generic;
using System.Linq;

namespace Teststation.Models
{
    public static class TestAnswerTransformer
    {
        public static TestAnswerViewModel TransformToTestAnswerViewModel(Test test, Session session)
        {
            var viewModel = new TestAnswerViewModel();
            viewModel.TestId = test.Id;
            viewModel.Topic = test.Topic;
            viewModel.IsStarted = session != null;
            viewModel.Questions = new List<QuestionAnswerViewModel>();

            foreach (var question in test.Questions
                .Where(x => x is MathQuestion)
                .Select(x => x as MathQuestion)
                .ToList())
            {
                viewModel.Questions.Add(new QuestionAnswerViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    Type = QuestionType.MathQuestion
                });
            }

            foreach (var question in test.Questions
               .Where(x => x is MultipleChoiceQuestion)
               .Select(x => x as MultipleChoiceQuestion)
               .ToList())
            {
                viewModel.Questions.Add(new QuestionAnswerViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    Choices = Choices(question.Choices),
                    Type = QuestionType.MultipleChoiceQuestion
                });
            }

            foreach (var question in test.Questions
               .Where(x => x is CircuitQuestion)
               .Select(x => x as CircuitQuestion)
               .ToList())
            {
                viewModel.Questions.Add(new QuestionAnswerViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    CircuitParts = CircuitParts(question.Parts),
                    Amperage = question.Amperage,
                    CircuitType = question.CircuitType,
                    InitialCurrent = question.InitialCurrent,
                    Type = QuestionType.CircuitQuestion
                });
            }

            viewModel.Questions = viewModel.Questions.OrderBy(x => x.Position).ToList();

            return viewModel;
        }

        private static List<ChoiceAnswerViewModel> Choices(List<Choice> choices)
        {
            var list = new List<ChoiceAnswerViewModel>();
            foreach (var choice in choices)
            {
                list.Add(new ChoiceAnswerViewModel
                {
                    Id = choice.Id,
                    QuestionId = choice.QuestionId,
                    Correct = choice.Correct,
                    Text = choice.Text,
                });
            }
            return list;
        }

        private static List<CircuitPartAnswerViewModel> CircuitParts(List<CircuitPart> parts)
        {
            var list = new List<CircuitPartAnswerViewModel>();
            foreach (var part in parts)
            {
                list.Add(new CircuitPartAnswerViewModel
                {
                    Id = part.Id,
                    QuestionId = part.QuestionId,
                    NeededCurrent = part.NeededCurrent,
                    Position = part.Position,
                    Resistance = part.Resistance,
                    Resistor1 = new ResistorAnswerViewModel
                    {
                        CircuitPartId = part.Id,
                        CorrectResistance = part.Resistor1.CorrectResistance,
                        Id = part.Resistor1.Id,
                        Visible = part.Resistor1.Visible
                    },
                    Resistor2 = new ResistorAnswerViewModel
                    {
                        CircuitPartId = part.Id,
                        CorrectResistance = part.Resistor2.CorrectResistance,
                        Id = part.Resistor2.Id,
                        Visible = part.Resistor2.Visible
                    },
                    Resistor3 = new ResistorAnswerViewModel
                    {
                        CircuitPartId = part.Id,
                        CorrectResistance = part.Resistor3.CorrectResistance,
                        Id = part.Resistor3.Id,
                        Visible = part.Resistor3.Visible
                    },
                });
            }
            return list;
        }
    }
}
