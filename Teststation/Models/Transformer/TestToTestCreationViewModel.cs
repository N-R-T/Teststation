using System.Collections.Generic;
using System.Linq;

namespace Teststation.Models
{
    public static class TestCreationTransformer
    {
        public static TestCreationViewModel TransformToTestCreationViewModel(Test test)
        {
            var viewModel = new TestCreationViewModel();
            viewModel.Id = test.Id;
            viewModel.Topic = test.Topic;
            viewModel.Questions = new List<QuestionCreationViewModel>();
            foreach (var question in test.Questions
                .Where(x => x is MathQuestion)
                .Select(x => x as MathQuestion)
                .ToList())
            {
                viewModel.Questions.Add(new QuestionCreationViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    CorrectAnswer = question.CorrectAnswer,
                    Type = QuestionType.MathQuestion
                });
            }

            foreach (var question in test.Questions
               .Where(x => x is MultipleChoiceQuestion)
               .Select(x => x as MultipleChoiceQuestion)
               .ToList())
            {
                viewModel.Questions.Add(new QuestionCreationViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    Choices = question.Choices,
                    Type = QuestionType.MultipleChoiceQuestion
                });
            }

            foreach (var question in test.Questions
               .Where(x => x is CircuitQuestion)
               .Select(x => x as CircuitQuestion)
               .ToList())
            {
                var parts = new List<CircuitPart>();
                foreach (var part in question.Parts)
                {
                    parts.Add(new CircuitPart
                    {
                        Id = part.Id,
                        Resistance = part.Resistance,
                        NeededCurrent = part.NeededCurrent,
                        Position = part.Position,
                        QuestionId = part.QuestionId,
                        Resistor1 = part.Resistor1,
                        Resistor2 = part.Resistor2,
                        Resistor3 = part.Resistor3
                    });
                }

                viewModel.Questions.Add(new QuestionCreationViewModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    TestId = question.TestId,
                    Position = question.Position,
                    Points = question.Points,
                    CircuitParts = parts,
                    Amperage = question.Amperage,
                    InitialCurrent = question.InitialCurrent,
                    CircuitType = question.CircuitType,
                    Type = QuestionType.CircuitQuestion
                });
            }

            viewModel.Questions = viewModel.Questions.OrderBy(x => x.Position).ToList();

            foreach (var question in viewModel.Questions)
            {
                question.Test = test;
            }

            return viewModel;
        }
        public static Test TransformToTest(TestCreationViewModel viewModel)
        {
            var test = new Test();
            test.Id = viewModel.Id;
            test.Topic = viewModel.Topic;
            test.Questions = new List<Question>();
            if (viewModel.Questions != null)
            {
                foreach (var question in viewModel.Questions
                  .Where(x => x.Type == QuestionType.MathQuestion)
                  .ToList())
                {
                    test.Questions.Add(new MathQuestion
                    {
                        Id = question.Id,
                        Text = question.Text,
                        TestId = question.TestId,
                        Position = question.Position,
                        Points = question.Points,
                        CorrectAnswer = question.CorrectAnswer
                    });
                }

                foreach (var question in viewModel.Questions
                   .Where(x => x.Type == QuestionType.MultipleChoiceQuestion)
                   .ToList())
                {
                    test.Questions.Add(new MultipleChoiceQuestion
                    {
                        Id = question.Id,
                        Text = question.Text,
                        TestId = question.TestId,
                        Position = question.Position,
                        Points = question.Points,
                        Choices = question.Choices
                    });
                }

                foreach (var question in viewModel.Questions
                   .Where(x => x.Type == QuestionType.CircuitQuestion)
                   .ToList())
                {
                    var parts = new List<CircuitPart>();
                    foreach (var part in question.CircuitParts)
                    {
                        parts.Add(new CircuitPart
                        {
                            Id = part.Id,
                            Resistance = part.Resistance,
                            NeededCurrent = part.NeededCurrent,
                            QuestionId = part.QuestionId,
                            Position = part.Position,
                            Resistor1 = part.Resistor1,
                            Resistor2 = part.Resistor2,
                            Resistor3 = part.Resistor3,
                            Resistor1Id = part.Resistor1.Id,
                            Resistor2Id = part.Resistor2.Id,
                            Resistor3Id = part.Resistor3.Id
                        });
                    }
                    test.Questions.Add(new CircuitQuestion
                    {
                        Id = question.Id,
                        Text = question.Text,
                        TestId = question.TestId,
                        Position = question.Position,
                        Points = question.Points,
                        Parts = parts,
                        CircuitType = question.CircuitType,
                        Amperage = question.Amperage,
                        InitialCurrent = question.InitialCurrent
                    });
                }
            }

            return test;
        }
    }
}
