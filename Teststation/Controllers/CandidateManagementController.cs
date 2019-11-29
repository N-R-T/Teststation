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
    public class CandidateManagementController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private Database _context;
        public CandidateManagementController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }
        public IActionResult CandidateList()
        {
            var candidates = _context.UserInformation                
                .Where(x=>x.Role == UserRole.Candidate)                
                .ToList();

            var candidateList = new List<CandidateListEntryViewModel>();
            foreach (var candidate in candidates)
            {
                candidateList.Add(new CandidateListEntryViewModel {
                     UserInformation = candidate,
                     UserId = candidate.User.Id, //<-- context
                     Name = candidate.User.UserName,
                });
            }
            return View(candidateList);
        }

        public IActionResult CandidateDetails(long? id)
        {
            return View();
        }
    }
}