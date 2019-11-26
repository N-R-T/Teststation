using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class TestAnswerController : Controller
    {
        private readonly Database _context;
        private static DateTime StartTime;

        public TestAnswerController(Database context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(long? testId, long? userId)
        {
            if(testId == null || userId == null)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var session = _context.Sessions.FirstOrDefault(x=>x.TestId == testId && x.CandidateId == userId);
            if (session.Completed)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            StartTime = DateTime.Now;
            return View(GetViewModel((long)testId, (long)userId));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Break(long id, [Bind("UserId,TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, false);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(long id, [Bind("UserId,TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, true);
            return RedirectToAction(nameof(Index));
        }

        private TestAnswerViewModel GetViewModel(long testId, long userId)
        {
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            test.Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList();
            foreach (var question in test.Questions
               .Where(x => x is MultipleChoiceQuestion)
               .Select(x => x as MultipleChoiceQuestion)
               .ToList())
            {
                question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
            }
            var viewModel = TestAnswerTransformer.TransformToTestAnswerViewModel(test, userId);

            foreach (var question in viewModel.Questions
                  .Where(x => x.Type == "MathQuestion")
                  .ToList())
            {
                question.GivenAnswer = _context.MathAnswers.FirstOrDefault(x => x.QuestionId == question.Id && x.CandidateId == userId).GivenAnswer;
            }

            foreach (var question in viewModel.Questions
                 .Where(x => x.Type == "MultipleChoiceQuestion")
                 .ToList())
            {
                foreach (var answer in question.Choices)
                {
                    var answerDB = _context.MultipleChoiceAnswers.FirstOrDefault(x => x.ChoiceId == answer.Id && x.CandidateId == userId);
                    answer.Correct = answerDB != null;
                }
            }
            return viewModel;
        }

        private void SaveSession(TestAnswerViewModel model, bool Completed)
        {
            foreach (var question in model.Questions
                  .Where(x => x.Type == "MathQuestion")
                  .ToList())
            {
                var answerDB = _context.MathAnswers
                              .FirstOrDefault(x => x.CandidateId == model.UserId &&
                                   x.QuestionId == question.Id);
                if (answerDB == null)
                {
                    _context.Answers.Add(new MathAnswer { CandidateId = model.UserId, QuestionId = question.Id, GivenAnswer = question.GivenAnswer });
                }
                else
                {
                    answerDB.GivenAnswer = question.GivenAnswer;
                    _context.MathAnswers.Update(answerDB);
                }
            }
            foreach (var question in model.Questions
               .Where(x => x.Type == "MultipleChoiceQuestion")
               .ToList())
            {
                foreach (var answer in question.Choices)
                {
                    var answerDB = _context.MultipleChoiceAnswers
                              .FirstOrDefault(x => x.CandidateId == model.UserId &&
                                   x.ChoiceId == answer.Id);
                    if (answer.Correct)
                    {
                        if (answerDB == null)
                        {
                            _context.Answers.Add(new MultipleChoiceAnswer { CandidateId = model.UserId, ChoiceId = answer.Id });
                        }
                    }
                    else
                    {
                        if (answerDB != null)
                        {
                            _context.Answers.Remove(answerDB);
                        }
                    }
                }
            }
            var session = _context.Sessions
                              .FirstOrDefault(x => x.CandidateId == model.UserId &&
                                   x.TestId == model.TestId);
            if (session == null)
            {
                _context.Sessions.Add(new Session { CandidateId = model.UserId, TestId = model.TestId, Completed = Completed, Duration = new System.TimeSpan() });
            }
            else
            {
                session.Completed = Completed;
                session.Duration += (DateTime.Now - StartTime);
            }

            _context.SaveChanges();
        }


        public PartialViewResult QuestionHead(QuestionAnswerViewModel question)
        {
            return PartialView(question);
        }

        public PartialViewResult MultipleChoiceQuestion(QuestionAnswerViewModel question)
        {
            return PartialView(question);
        }

    }
}