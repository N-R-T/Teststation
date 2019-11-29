using Microsoft.AspNetCore.Identity;
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
        private SignInManager<IdentityUser> _signManager;
        private UserManager<IdentityUser> _userManager;
        private readonly Database _context;
        private static DateTime StartTime;

        public TestAnswerController(Database context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        public async Task<IActionResult> Index(long? testId)
        {
            if (testId == null)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x=>x.UserId ==  _userManager.GetUserId(User));
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);

            if (test.ReleaseStatus == TestStatus.InProgress)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            if (session != null)
            {
                if (session.Completed)
                {
                    return RedirectToAction("Index", "Home", 0);
                }
            }

            StartTime = DateTime.Now;
            return View(GetViewModel(test, session));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Break(long id, [Bind("TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, false);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(long id, [Bind("TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, true);
            return RedirectToAction("Index","Evaluation", new{ testId = model.TestId});
        }

        private TestAnswerViewModel GetViewModel(Test test, Session session)
        {
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User));
            test.Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList();
            foreach (var question in test.Questions
               .Where(x => x is MultipleChoiceQuestion)
               .Select(x => x as MultipleChoiceQuestion)
               .ToList())
            {
                question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
            }
            var viewModel = TestAnswerTransformer.TransformToTestAnswerViewModel(test, session);
            
            if (viewModel.IsStarted)
            {
                foreach (var question in viewModel.Questions
                      .Where(x => x.Type == "MathQuestion")
                      .ToList())
                {
                    question.GivenAnswer = _context.MathAnswers.FirstOrDefault(x => x.QuestionId == question.Id && x.CandidateId == user.Id).GivenAnswer;
                }

                foreach (var question in viewModel.Questions
                     .Where(x => x.Type == "MultipleChoiceQuestion")
                     .ToList())
                {
                    foreach (var answer in question.Choices)
                    {
                        var answerDB = _context.MultipleChoiceAnswers.FirstOrDefault(x => x.ChoiceId == answer.Id && x.CandidateId == user.Id);
                        answer.Correct = answerDB != null;
                    }
                }
            }

            return viewModel;
        }

        private void SaveSession(TestAnswerViewModel model, bool Completed)
        {
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User));
            foreach (var question in model.Questions
                  .Where(x => x.Type == "MathQuestion")
                  .ToList())
            {
                var answerDB = _context.MathAnswers
                              .FirstOrDefault(x => x.CandidateId == user.Id &&
                                   x.QuestionId == question.Id);
                if (answerDB == null)
                {
                    _context.Answers.Add(new MathAnswer { CandidateId = user.Id, QuestionId = question.Id, GivenAnswer = question.GivenAnswer });
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
                              .FirstOrDefault(x => x.CandidateId == user.Id &&
                                   x.ChoiceId == answer.Id);
                    if (answer.Correct)
                    {
                        if (answerDB == null)
                        {
                            _context.Answers.Add(new MultipleChoiceAnswer { CandidateId = user.Id, ChoiceId = answer.Id });
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
                              .FirstOrDefault(x => x.CandidateId == user.Id &&
                                   x.TestId == model.TestId);
            if (session == null)
            {
                _context.Sessions.Add(new Session { CandidateId = user.Id, TestId = model.TestId, Completed = Completed, Duration = new System.TimeSpan() });
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