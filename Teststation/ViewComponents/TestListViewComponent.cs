using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;

namespace Teststation.ViewComponents
{
    public class TestListViewComponent : ViewComponent
    {
        private SignInManager<User> _signManager;
        private UserManager<User> _userManager;
        private readonly Database _context;

        public TestListViewComponent(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new CandidateSessionViewModel(_context, _userManager.GetUserId(UserClaimsPrincipal));
            return View(viewModel);
        }
    }
}
