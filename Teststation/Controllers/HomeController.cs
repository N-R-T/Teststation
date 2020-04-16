using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Teststation.Models;
using Teststation.Models.ViewModels;


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
            if (!_signManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            DeleteBackUpTest();
            DeleteUnusedResistors();
            DeleteOldAccounts();
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
        private void DeleteUnusedResistors()
        {            
            foreach (var resistor in _context.Resistors)
            {
                if (!_context.CircuitParts.Any(x=>
                x.Resistor1Id == resistor.Id ||
                x.Resistor2Id == resistor.Id ||
                x.Resistor3Id == resistor.Id ))
                {
                    _context.Resistors.Remove(resistor);
                }
            }
            _context.SaveChanges();
        }
        private void DeleteOldAccounts()
        {
            var oldAccounts = _userManager.GetUsersInRoleAsync(Consts.Candidate).Result.Where(x => x.DayOfLastActivity.AddYears(1) <= DateTime.Now);
            foreach (var user in oldAccounts)
            {
                
                user.UserName =
                user.NormalizedUserName =
                user.Email =
                user.NormalizedEmail =
                user.PasswordHash = null;
                user.IsDeleted = true;
                _userManager.RemoveFromRolesAsync(user, new List<string>() { Consts.Admin, Consts.Candidate });
                _context.Users.Update(user);
            }
            _context.SaveChanges();
        }
                
    }
}
