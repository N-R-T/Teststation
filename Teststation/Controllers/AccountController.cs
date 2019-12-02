using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;
using Teststation.Models.ViewModels;
using User = Microsoft.AspNetCore.Identity.IdentityUser;

namespace Teststation.Controllers
{
    public class AccountController : Controller
    {
        private SignInManager<User> _signManager;
        private UserManager<User> _userManager;
        private Database _context;

        public AccountController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        [HttpGet]
        public ViewResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(x => x.UserName == model.Username))
                {
                    return View();
                }
                var user = new User { UserName = model.Username, Email = model.Username };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var userInformation = new UserInformation
                    {
                        Role = UserRole.Candidate,
                        UserId = user.Id,
                        DayOfLastActivity = DateTime.Now,
                    };
                    _context.UserInformation.Add(userInformation);
                    _context.SaveChanges();
                    return RedirectToAction("CandidateList", "CandidateManagement");
                }
            }
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await _signManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signManager.PasswordSignInAsync(model.Username,
                           model.Password, true, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var userInformation = _context.UserInformation.FirstOrDefault(x => x.User.UserName == model.Username);
                    userInformation.DayOfLastActivity = DateTime.Now;
                    _context.UserInformation.Update(userInformation);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }
    }
}
