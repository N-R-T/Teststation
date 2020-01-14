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
        private SignInManager<IdentityUser> _signManager;
        private UserManager<IdentityUser> _userManager;
        private readonly Database _context;

        public TestListViewComponent(Database context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signManager)
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
