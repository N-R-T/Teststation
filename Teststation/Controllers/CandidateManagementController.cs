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
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!_context.Users.Any(x=>x.Id == id))
            {
                return RedirectToAction("CandidateList");
            }
            var viewModel = new CandidateSessionViewModel(_context, id);           
            return View(viewModel);
        }

        public async Task<IActionResult> DeleteCandidate(string id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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

        private bool IsNotAdmin()
        {
            if (!_signManager.IsSignedIn(User))
            {
                return true;
            }
            if (_context.UserInformation.Any(x => x.UserId == _userManager.GetUserId(User)))
            {
                return _context.UserInformation.First(x => x.UserId == _userManager.GetUserId(User)).Role != UserRole.Admin;
            }
            return false;
        }
    }
}