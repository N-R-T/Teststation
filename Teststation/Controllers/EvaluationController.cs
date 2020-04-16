using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class EvaluationController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private readonly Database _context;

        public EvaluationController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        [Authorize(Roles = Consts.Candidate)]
        public IActionResult Index(long? testId)
        {
            if (!_signManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            if (!TestIsValid(testId))
            {
                return RedirectToAction("Index", "Home");
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.Users.FirstOrDefault(x => x.Id == _userManager.GetUserId(User));

            if (!SessionIsValid(testId, user.Id))
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
        }

        [Authorize(Roles = Consts.Admin)]
        public IActionResult IndexAdmin(long? testId, string userId)
        {
            if (!ParametersAreValid(testId, userId))
            {
                return RedirectToAction("Index", "Home");
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
        }

        private bool ParametersAreValid(long? testId, string userId)
        {
            if (!TestIsValid(testId))
            {
                return false;
            }
            if (!UserIsValid(userId))
            {
                return false;
            }
            if (!SessionIsValid(testId, userId))
            {
                return false;
            }

            return true;
        }

        private bool TestIsValid(long? testId)
        {
            if (testId == null)
            {
                return false;
            }
            if (_context.Tests.Any(x => x.Id == testId))
            {
                return _context.Tests.First(x => x.Id == testId).ReleaseStatus != TestStatus.InProgress;
            }
            return _context.Tests.Any(x => x.Id == testId);
        }

        private bool UserIsValid(string userId)
        {
            return _context.Users.Any(x => x.Id == userId);
        }

        private bool SessionIsValid(long? testId, string userId)
        {
            if (_context.Sessions.Any(x => x.TestId == testId && x.CandidateId == userId))
            {
                return _context.Sessions.First(x => x.TestId == testId && x.CandidateId == userId).Completed;
            }
            return _context.Sessions.Any(x => x.TestId == testId && x.CandidateId == userId);
        }

        public PartialViewResult QuestionHead(Answer answer)
        {
            return PartialView(answer);
        }

        public PartialViewResult MathQuestion(MathAnswer answer)
        {
            return PartialView(answer);
        }

        public PartialViewResult MultipleChoiceQuestion(MultipleChoiceEvalutionViewModel answers)
        {
            return PartialView(answers);
        }
    }
}