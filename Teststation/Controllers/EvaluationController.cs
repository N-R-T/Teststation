using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class EvaluationController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signManager;
        private readonly Database _context;

        public EvaluationController(Database context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        public IActionResult Index(long? testId)
        {
            if (!_signManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!TestIsValid(testId))
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User));

            if (!SessionIsValid(testId, user.Id))
            {
                return RedirectToAction("Index", "Home", 0);
            }

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
        }

        public IActionResult IndexAdmin(long? testId, long? userId)
        {
            if (!ParametersAreValid(testId, userId))
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x => x.Id == userId);

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
        }

        private bool ParametersAreValid(long? testId, long? userId)
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
            return _context.Tests.Any(x => x.Id == testId);
        }

        private bool UserIsValid(long? userId)
        {
            return _context.UserInformation.Any(x => x.Id == userId);
        }

        private bool SessionIsValid(long? testId, long? userId)
        {
            if (_context.Sessions.Any(x => x.TestId == testId && x.CandidateId == userId))
            {
                return _context.Sessions.First(x => x.TestId == testId && x.CandidateId == userId).Completed;
            }
            return true;
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