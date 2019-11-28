using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class EvaluationController : Controller
    {
        private readonly Database _context;

        public EvaluationController(Database context)
        {
            _context = context;
        }

        public IActionResult Index(long? userId, long? testId)
        {
            if (testId == null || userId == null)
            {
                return RedirectToAction("Index", "Home", 0);
            }
            var test = _context.Tests.FirstOrDefault(x => x.Id == testId);
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == userId);
                        
            if (session != null)
            {
                if (!session.Completed)
                {
                    return RedirectToAction("Index", "Home", 0);
                }
            }

            var viewModel = new EvaluationViewModel(test, (long)userId, _context);
            return View(viewModel);
        }

        public PartialViewResult QuestionHead(Answer answer)
        {
            return PartialView(answer);
        }

        public PartialViewResult MathQuestion(MathAnswer answer)
        {
            return PartialView(answer);
        }

        public PartialViewResult MultipleChoiceQuestion(MultipleChoiceEvalutionViewModel answers)
        {
            return PartialView(answers);
        }
    }
}