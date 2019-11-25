using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Teststation.Models;

namespace Teststation.Controllers
{
    public class TestCreationController : Controller
    {
        private readonly Database _context;

        public TestCreationController(Database context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Tests.ToListAsync());
        }

        #region Neuen Test erstellen
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Topic")] Test test)
        {
            if (ModelState.IsValid)
            {
                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }
        #endregion

        #region Test bearbeiten
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            test.Questions = await _context.Questions.Where(x=>x.TestId == test.Id).ToListAsync();
            if (test.Questions == null)
            {
                test.Questions = new List<Question>();
            }
            foreach (MultipleChoiceQuestion question in test.Questions.Where(x=>x is MultipleChoiceQuestion))
            {
                question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
            }
            
            return View(TestCreationTransformer.TransformToTestCreationViewModel(test));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            var test = TestCreationTransformer.TransformToTest(model);
            if (id != test.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(test);
                    foreach (var question in test.Questions
                        .Where(x => x is MultipleChoiceQuestion)
                        .Select(x => x as MultipleChoiceQuestion))
                    {
                        _context.Update(question);
                        foreach (var choice in question.Choices)
                        {
                            _context.Update(choice);
                        }
                    }
                    foreach (var question in test.Questions
                        .Where(x => x is MathQuestion)
                        .Select(x => x as MathQuestion))
                    {
                        _context.Update(question);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> BackToMainMenu(long id)
        {
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddMathQuestion(long id)
        {
            var newQuestion = new MathQuestion();

            newQuestion.TestId = id;
            newQuestion.Position = _context.Questions.Where(x => x.TestId == id).Count();

            _context.Add(newQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id });
        }
        public async Task<IActionResult> AddMultipleChoiceQuestion(long id)
        {
            var newQuestion = new MultipleChoiceQuestion();

            newQuestion.TestId = id;
            newQuestion.Position = _context.Questions.Where(x => x.TestId == id).Count();

            _context.Add(newQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> AddChoice(long id)
        {
            var newChoice = new Choice();
            newChoice.QuestionId = id;
            _context.Add(newChoice);
            await _context.SaveChangesAsync();

            var question = await _context.Questions.FirstOrDefaultAsync(x=>x.Id == id);
            return RedirectToAction("Edit", new { id = question.TestId });
        }

        public async Task<IActionResult> DeleteQuestion(long id)
        {
            var oldQuestion = await _context.Questions.FindAsync(id);
            var test = await _context.Tests.FindAsync(oldQuestion.TestId);
            test.Questions = await _context.Questions.Where(x => x.TestId == test.Id).ToListAsync();
            test.Questions.Remove(oldQuestion);
            test.Questions = test.Questions.OrderBy(x => x.Position).ToList();
            int position = 0;

            foreach (var question in test.Questions)
            {
                question.Position = position;
                position++;
            }

            _context.Remove(oldQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { test.Id });
        }

        public async Task<IActionResult> DeleteChoice(long id)
        {
            var oldChoice = await _context.Choices.FindAsync(id);
            var question = await _context.Questions.FindAsync(oldChoice.QuestionId);
            var test = await _context.Tests.FindAsync(question.TestId); 
            _context.Remove(oldChoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = test.Id });
        }
        #endregion

        #region Löschen
        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var test = await _context.Tests.FindAsync(id);
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        private bool TestExists(long id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }

        public PartialViewResult QuestionHead(QuestionCreationViewModel question)
        {
            return PartialView(question);
        }
        public PartialViewResult MultipleChoiceQuestion(QuestionCreationViewModel question)
        {
            return PartialView(question);
        }
    }
}