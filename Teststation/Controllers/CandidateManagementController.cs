using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;
using Teststation.Models.ViewModels;
using User = Microsoft.AspNetCore.Identity.IdentityUser;

namespace Teststation.Controllers
{
    public class CandidateManagementController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private Database _context;
        public CandidateManagementController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }
        public IActionResult CandidateList()
        {
            var candidates = _context.UserInformation
                .Where(x => x.Role == UserRole.Candidate)
                .ToList();

            var candidateList = new List<CandidateListEntryViewModel>();
            foreach (var candidate in candidates)
            {
                candidate.User = _context.Users.FirstOrDefault(x => x.Id == candidate.UserId);
                candidateList.Add(new CandidateListEntryViewModel
                {
                    UserInformation = candidate,
                    UserId = candidate.User.Id,
                    Name = candidate.User.UserName,
                });
            }
            return View(candidateList);
        }

        public IActionResult CandidateDetails(string id)
        {
            var viewModel = new CandidateSessionViewModel();
            var testList = new List<TestCandidateViewModel>();
            var tests = _context.Tests.Where(x => x.ReleaseStatus == TestStatus.Public).ToList();
            viewModel.UserInformation = _context.UserInformation.FirstOrDefault(x => x.UserId == id);
            viewModel.UserInformation.User = _context.Users.FirstOrDefault(x => x.Id == id);

            foreach (var test in tests)
            {
                var session = _context.Sessions.FirstOrDefault(x => x.TestId == test.Id && x.CandidateId == viewModel.UserInformation.Id);
                var testRow = new TestCandidateViewModel();

                testRow.Test = test;
                testRow.IsStarted = session != null;


                if (testRow.IsStarted)
                {
                    var evaluation = new EvaluationViewModel(test, viewModel.UserInformation.Id, _context);
                    testRow.Result = Consts.resultIfEvaluationHasErrors;
                    if (evaluation.Answers != null && evaluation.Answers.Count != 0)
                    {
                        testRow.Result = evaluation.GetPercentage();
                    }
                    testRow.Duration = session.Duration;
                    testRow.Completed = session.Completed;
                }
                else
                {
                    testRow.Duration = new TimeSpan();
                    testRow.Result = 0;
                    testRow.Completed = false;
                }

                testList.Add(testRow);
            }

            viewModel.Tests = testList;
            return View(viewModel);
        }

        public async Task<IActionResult> DeleteCandidate(string id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            user.UserName =
                user.NormalizedUserName =
                user.Email =
                user.NormalizedEmail =
                user.PasswordHash = null;
            _context.Users.Update(user);

            var userInformation = _context.UserInformation.FirstOrDefault(x => x.UserId == id);
            userInformation.Role = UserRole.Deleted;
            _context.UserInformation.Update(userInformation);

            _context.SaveChanges();
            return RedirectToAction(nameof(CandidateList));
        }

        public async Task<IActionResult> DeleteSession(long testId, string userId)
        {
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == userId);
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);
            var mathAnswers = _context.MathAnswers.Where(x => x.CandidateId == user.Id && x.Question.TestId == testId);
            var multipleChoiceAnswers = _context.MultipleChoiceAnswers.Where(x => x.CandidateId == user.Id && x.Choice.Question.TestId == testId);

            _context.MathAnswers.RemoveRange(mathAnswers);
            _context.MultipleChoiceAnswers.RemoveRange(multipleChoiceAnswers);
            _context.Sessions.Remove(session);
            _context.SaveChanges();
            return RedirectToAction("CandidateDetails", new { id = userId });
        }
    }
}