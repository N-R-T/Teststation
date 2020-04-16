using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class TestAnswerController : Controller
    {
        private SignInManager<User> _signManager;
        private UserManager<User> _userManager;
        private readonly Database _context;
        private static DateTime StartTime;

        public TestAnswerController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        [Authorize(Roles = Consts.Candidate)]
        public async Task<IActionResult> Index(long? testId)
        {
            if (!TestIsValid(testId))
            {
                return RedirectToAction("Index", "Home");
            }

            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = await _userManager.GetUserAsync(User);

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
        private bool SessionIsValid(long? testId, string userId)
        {
            if (_context.Sessions.Any(x => x.TestId == testId && x.CandidateId == userId))
            {
                if (_context.Sessions.First(x => x.TestId == testId && x.CandidateId == userId).Completed)
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
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = Consts.Candidate)]
        private TestAnswerViewModel GetViewModel(Test test, Session session)
        {
            var user = _userManager.GetUserAsync(User).Result;
            test.Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList();

            foreach (var question in test.Questions
               .Where(x => x is MultipleChoiceQuestion)
               .Select(x => x as MultipleChoiceQuestion)
               .ToList())
            {
                question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
            }

            foreach (var question in test.Questions
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

            var viewModel = TestAnswerTransformer.TransformToTestAnswerViewModel(test, session);

            if (viewModel.IsStarted)
            {
                foreach (var question in viewModel.Questions
                      .Where(x => x.Type == QuestionType.MathQuestion)
                      .ToList())
                {
                    var answer = _context.MathAnswers.FirstOrDefault(x => x.QuestionId == question.Id && x.CandidateId == user.Id);
                    if (answer != null)
                    {
                        question.GivenAnswer = answer.GivenAnswer;
                    }
                    else
                    {
                        question.GivenAnswer = String.Empty;
                    }
                }

                foreach (var question in viewModel.Questions
                     .Where(x => x.Type == QuestionType.MultipleChoiceQuestion)
                     .ToList())
                {
                    foreach (var answer in question.Choices)
                    {
                        var answerDB = _context.MultipleChoiceAnswers
                            .FirstOrDefault(x => x.ChoiceId == answer.Id &&
                                            x.CandidateId == user.Id);
                        if (answerDB != null)
                        {
                            answer.Correct = true;
                        }
                        else
                        {
                            answer.Correct = false;
                        }
                    }
                }

                foreach (var question in viewModel.Questions
                     .Where(x => x.Type == QuestionType.CircuitQuestion)
                     .ToList())
                {
                    foreach (var part in question.CircuitParts)
                    {
                        var resistorId = part.Resistors().First(x => x.Visible == false).Id;
                        var answer = _context.CircuitAnswers.FirstOrDefault(x => x.ResistorId == resistorId && x.CandidateId == user.Id);
                        if (answer != null)
                        {
                            part.GivenResistance = answer.GivenResistance;
                        }
                        else
                        {
                            part.GivenResistance = 0;
                        }
                    }
                }
            }

            return viewModel;
        }

        [Authorize(Roles = Consts.Candidate)]
        private void SaveSession(TestAnswerViewModel model, bool Completed)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var mathAnswers = model.Questions
                  .Where(x => x.Type == QuestionType.MathQuestion)
                  .ToList();

            var multipleChoiceAnswers = model.Questions
               .Where(x => x.Type == QuestionType.MultipleChoiceQuestion)
               .ToList();

            var circuitAnswers = model.Questions
               .Where(x => x.Type == QuestionType.CircuitQuestion)
               .ToList();

            SaveResultsOfMathQuestions(mathAnswers, user);
            SaveResultsOfMultipleChoiceQuestions(multipleChoiceAnswers, user);
            SaveResultsOfCircuitQuestions(circuitAnswers, user);
            _context.SaveChanges();

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

        private void SaveResultsOfMathQuestions(List<QuestionAnswerViewModel> questions, User user)
        {
            foreach (var question in questions)
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
        }

        private void SaveResultsOfMultipleChoiceQuestions(List<QuestionAnswerViewModel> questions, User user)
        {
            foreach (var question in questions)
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
        }

        private void SaveResultsOfCircuitQuestions(List<QuestionAnswerViewModel> questions, User user)
        {
            foreach (var question in questions)
            {
                foreach (var part in question.CircuitParts)
                {
                    var resistorId = part.Resistors().First(x => x.Visible == false).Id;
                    var answerDB = _context.CircuitAnswers
                             .FirstOrDefault(x => x.CandidateId == user.Id &&
                                  x.ResistorId == resistorId);
                    if (answerDB == null)
                    {
                        _context.Answers.Add(new CircuitAnswer { CandidateId = user.Id, ResistorId = resistorId, GivenResistance = part.GivenResistance });
                    }
                    else
                    {
                        answerDB.GivenResistance = part.GivenResistance;
                        _context.CircuitAnswers.Update(answerDB);
                    }
                }

            }
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