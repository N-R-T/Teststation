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
            if (testId == null)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User));
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);

            if (session != null)
            {
                if (!session.Completed)
                {
                    return RedirectToAction("Index", "Home", 0);
                }
            }

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
        }

        public IActionResult IndexAdmin(long? testId, long? userId)
        {
            if (testId == null)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var user = _context.UserInformation.FirstOrDefault(x => x.Id == userId);
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);

            if (session != null)
            {
                if (!session.Completed)
                {
                    return RedirectToAction("Index", "Home", 0);
                }
            }

            var viewModel = new EvaluationViewModel(test, user.Id, _context);
            return View(viewModel);
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