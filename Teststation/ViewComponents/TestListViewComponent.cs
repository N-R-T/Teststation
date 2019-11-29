using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
            var testList = new List<TestCandidateViewModel>();
            var tests = await _context.Tests.Where(x => x.ReleaseStatus == TestStatus.Public).ToListAsync();
            var user = _context.UserInformation.FirstOrDefault(x => x.UserId == _userManager.GetUserId(UserClaimsPrincipal));

            foreach (var test in tests)
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(x => x.TestId == test.Id && x.CandidateId == user.Id);
                var testRow = new TestCandidateViewModel();

                testRow.Test = test;
                testRow.IsStarted = session != null;
                
                if (testRow.IsStarted)
                {
                    testRow.Result = new EvaluationViewModel(test, user.Id, _context).GetPercentage();
                    testRow.Completed = session.Completed;
                }
                else
                {
                    testRow.Result = 0;
                    testRow.Completed = false;
                }

                testList.Add(testRow);
            }
            return View(testList);
        }
    }
}
