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
        private readonly Database _context;

        public TestListViewComponent(Database context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _context.Tests.Where(x => x.ReleaseStatus == TestStatus.Public).ToListAsync());
        }
    }
}
