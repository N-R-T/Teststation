using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Teststation.Models;
using Teststation.Models.ViewModels;
using User = Microsoft.AspNetCore.Identity.IdentityUser;


namespace Teststation.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private Database _context;
        public HomeController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        public IActionResult Index()
        {
            DeleteBackUpTest();
            DeleteOldAccounts();
            if (!_signManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            var viewModel = Quotes.GetCurrentQoute(_userManager.GetUserName(User));
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult EasterEggs()
        {
            var viewModel = Quotes.GetAllQuotes(_userManager.GetUserName(User));
            return View(viewModel);
        }

        private void DeleteBackUpTest()
        {
            if (_context.Tests.Any(x => x.Id == Consts.backUpTestId))
            {
                _context.Tests.Remove(_context.Tests.FirstOrDefault(x => x.Id == Consts.backUpTestId));
            }
            _context.SaveChanges();
        }
        private void DeleteOldAccounts()
        {
            var oldAccounts = _context.UserInformation.Where(x => x.Role == UserRole.Candidate && x.DayOfLastActivity.AddYears(1) <= DateTime.Now);
            foreach (var userInformation in oldAccounts)
            {
                userInformation.Role = UserRole.Deleted;
                var user = _context.Users.FirstOrDefault(x => x.Id == userInformation.UserId);
                user.UserName =
                user.NormalizedUserName =
                user.Email =
                user.NormalizedEmail =
                user.PasswordHash = null;
                _context.Users.Update(user);
                _context.UserInformation.Update(userInformation);
            }
            _context.SaveChanges();
        }
    }
}
