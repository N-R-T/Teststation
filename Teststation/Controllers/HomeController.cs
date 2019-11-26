using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class HomeController : Controller
    {
        private readonly Database _context;
        public HomeController(Database context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (_context.Tests.Any(x => x.Id == Consts.backUpTestId))
            {
                _context.Tests.Remove(_context.Tests.FirstOrDefault(x => x.Id == Consts.backUpTestId));
            }
            _context.SaveChanges();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
