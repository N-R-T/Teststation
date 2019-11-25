using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    Type = "MathQuestion"
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
                    Type = "MultipleChoiceQuestion"
                });
            }

            viewModel.Questions = viewModel.Questions.OrderBy(x=>x.Position).ToList();

            return viewModel;
        }
        public static Test TransformToTest(TestCreationViewModel viewModel)
        {
            var test = new Test();
            test.Id = viewModel.Id;
            test.Topic = viewModel.Topic;
            test.Questions = new List<Question>();
            foreach (var question in viewModel.Questions
                .Where(x => x.Type == "MathQuestion")
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
               .Where(x => x.Type == "MultipleChoiceQuestion")
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

            return test;
        }
    }
}
