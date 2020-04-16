using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;
using Teststation.Models.ViewModels;

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
        [Authorize(Roles = Consts.Admin)]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Authorize(Roles = Consts.Admin)]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //_userManager.IsInRoleAsync(_userManager.GetUserAsync(User).Result, Consts.Admin);

            if (ModelState.IsValid && !_context.Users.Any(x => x.UserName == model.Username))
            {
                var user = new User { UserName = model.Username, Email = model.Username, DayOfLastActivity = DateTime.Now };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Consts.Candidate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("CandidateList", "CandidateManagement");
                }
            }
            model.RegisterErrors(_context);
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
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
                    var user = _context.Users.First(x=>x.UserName == model.Username);
                    user.DayOfLastActivity = DateTime.Now;
                    _context.Update(user);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt");
            model.LoginErrors(_context);
            return View(model);
        }        
    }
}
