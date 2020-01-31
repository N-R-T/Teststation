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
                    Type = "MathQuestion"
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
                    Type = "MultipleChoiceQuestion"
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
    }
}
