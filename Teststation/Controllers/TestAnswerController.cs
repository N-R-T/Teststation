using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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
            if (!_signManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!TestIsValid(testId))
            {
                return RedirectToAction("Index", "Home", 0);
            }
            if (!UserIsValid())
            {
                return RedirectToAction("Index", "Home");
            }

            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User));

            if (!SessionIsValid(testId, user.Id))
            {
                return RedirectToAction("Index", "Home");
            }

            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);
            StartTime = DateTime.Now;
            return View(GetViewModel(test, session));
        }

        private bool TestIsValid(long? testId)
        {
            if (_context.Tests.Any(x => x.Id == testId))
            {
                return _context.Tests.First(x => x.Id == testId).ReleaseStatus == TestStatus.Public;
            }
            return true;
        }
        private bool UserIsValid()
        {
            if (_context.UserInformation.Any(x => x.UserId == _userManager.GetUserId(User)))
            {
                return _context.UserInformation.First(x => x.UserId == _userManager.GetUserId(User)).Role == UserRole.Candidate;
            }
            return true;
        }
        private bool SessionIsValid(long? testId, long? userId)
        {
            if (_context.Sessions.Any(x => x.TestId == testId && x.CandidateId == userId))
            {
                if(_context.Sessions.First(x => x.TestId == testId && x.CandidateId == userId).Completed)
                {
                    return false;
                }                
            }
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Break(long id, [Bind("TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(long id, [Bind("TestId,SessionId,Questions")] TestAnswerViewModel model)
        {
            SaveSession(model, true);
            return RedirectToAction("Index", "Evaluation", new { testId = model.TestId });
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
                _context.Sessions.Add(new Session { CandidateId = user.Id, TestId = model.TestId, Completed = Completed, Duration = (DateTime.Now - StartTime) });
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