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
        public IActionResult Index()
        {
            return View(EvaluationViewModel.Filler());
        }

        //public IActionResult Index(long UserId, long TestId)
        //{
        //    return View(new EvaluationViewModel(UserId, TestId));
        //}

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