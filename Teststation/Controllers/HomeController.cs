using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Teststation.Models;
using Teststation.Models.ViewModels;
using User = Microsoft.AspNetCore.Identity.IdentityUser;
using Microsoft.AspNetCore.Authorization;


namespace Teststation.Controllers
{
    public class HomeController : Controller
    {
        private SignInManager<User> _signManager;
        private UserManager<User> _userManager;

        public HomeController(UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
        }

        //private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public IActionResult Index()
        {
            if (_signManager.IsSignedIn(User))
            {
                return View();
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
