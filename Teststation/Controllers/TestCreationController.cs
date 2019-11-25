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
        private static long originalTestId;

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
            if (id != Consts.backUpTestId)
            {
                originalTestId = (long)id;
                return RedirectToAction("Edit", new { id = Consts.backUpTestId });
            }

            var backUpTest = CreateBackUp();
            return View(TestCreationTransformer.TransformToTestCreationViewModel(backUpTest));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            //var test = TestCreationTransformer.TransformToTest(model);
            //if (id != test.Id)
            //{
            //    return NotFound();
            //}
            //try
            //{
            //    SaveTest(test);
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!TestExists(test.Id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            //if (ModelState.IsValid)
            //{

            //    return RedirectToAction(nameof(Index));
            //}
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveChanges(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            var test = TestCreationTransformer.TransformToTest(model);
            test.Id = originalTestId;
            _context.Questions.RemoveRange(_context.Questions.Where(x => x.TestId == originalTestId));
            _context.Choices.RemoveRange(_context.Choices
                .Include(x => x.Question)
                .Where(x => x.Question.TestId == originalTestId));

            foreach (var question in test.Questions)
            {
                question.TestId = test.Id;
            }

            _context.Update(test);

            _context.SaveChanges();
            DeleteBackUp();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> BackToMainMenu(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            DeleteBackUp();
            return RedirectToAction(nameof(Index));
        }

        private Test CreateBackUp()
        {
            var backUpTest = new Test();
            if (_context.Tests.Any(x => x.Id == Consts.backUpTestId))
            {
                backUpTest = _context.Tests.FirstOrDefault(x => x.Id == Consts.backUpTestId);
                backUpTest.Questions = _context.Questions.Where(x => x.TestId == Consts.backUpTestId).ToList();
                if (backUpTest.Questions == null)
                {
                    backUpTest.Questions = new List<Question>();
                }
                foreach (MultipleChoiceQuestion question in backUpTest.Questions.Where(x => x is MultipleChoiceQuestion))
                {
                    question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
                }
                if (backUpTest.Questions == null)
                {
                    backUpTest.Questions = new List<Question>();
                }
            }
            else
            {
                var test = _context.Tests.FirstOrDefault(x => x.Id == originalTestId);
                backUpTest = new Test(test);
                backUpTest.Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList();
                if (backUpTest.Questions == null)
                {
                    backUpTest.Questions = new List<Question>();
                }
                foreach (MultipleChoiceQuestion question in backUpTest.Questions.Where(x => x is MultipleChoiceQuestion))
                {
                    question.Choices = _context.Choices.Where(x => x.QuestionId == question.Id).ToList();
                }
                if (backUpTest.Questions == null)
                {
                    backUpTest.Questions = new List<Question>();
                }
                SaveBackUp(backUpTest);
            }

            return backUpTest;
        }
        private void DeleteBackUp()
        {
            if (_context.Tests.Any(x => x.Id == Consts.backUpTestId))
            {
                _context.Tests.Remove(_context.Tests.FirstOrDefault(x => x.Id == Consts.backUpTestId));
            }
            _context.SaveChanges();
        }

        private void SaveBackUp(Test backUpTest)
        {
            if (!_context.Tests.Any(x => x.Id == Consts.backUpTestId))
            {
                _context.Tests.Add(backUpTest);

                var multiChoiceQuestions = backUpTest.Questions
                    .Where(x => x is MultipleChoiceQuestion)
                    .Select(x => x as MultipleChoiceQuestion)
                   .ToList();
                foreach (var question in multiChoiceQuestions)
                {
                    var backUpQuestion = new MultipleChoiceQuestion
                    {
                        Text = question.Text,
                        Points = question.Points,
                        Position = question.Position,
                        TestId = originalTestId,
                        Choices = null
                    };
                    _context.Questions.Add(backUpQuestion);

                    var choices = question.Choices.ToList();
                    foreach (var choice in choices)
                    {
                        _context.Choices.Add(new Choice
                        {
                            Text = choice.Text,
                            Correct = choice.Correct,
                            QuestionId = backUpQuestion.Id
                        });
                    }
                }


                var mathQuestions = backUpTest.Questions
                    .Where(x => x is MathQuestion)
                    .Select(x => x as MathQuestion)
                    .ToList();
                foreach (var question in mathQuestions)
                {
                    var backUpQuestion = new MathQuestion
                    {
                        Text = question.Text,
                        Points = question.Points,
                        Position = question.Position,
                        TestId = originalTestId,
                        CorrectAnswer = question.CorrectAnswer
                    };
                    _context.Questions.Add(backUpQuestion);
                }
            }
            else
            {
                _context.Update(backUpTest);
                foreach (var question in backUpTest.Questions
                    .Where(x => x is MultipleChoiceQuestion)
                    .Select(x => x as MultipleChoiceQuestion))
                {
                    _context.Update(question);
                    foreach (var choice in question.Choices)
                    {
                        _context.Update(choice);
                    }
                }
                foreach (var question in backUpTest.Questions
                    .Where(x => x is MathQuestion)
                    .Select(x => x as MathQuestion))
                {
                    _context.Update(question);
                }
            }
            _context.SaveChanges();
        }

        public async void SaveTest(Test test)
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

        public async Task<IActionResult> AddMathQuestion(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var newQuestion = new MathQuestion();

            newQuestion.TestId = Consts.backUpTestId;
            newQuestion.Position = _context.Questions.Where(x => x.TestId == Consts.backUpTestId).Count();
            newQuestion.Points = 1;

            _context.Add(newQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id });
        }
        public async Task<IActionResult> AddMultipleChoiceQuestion(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var newQuestion = new MultipleChoiceQuestion();

            newQuestion.TestId = Consts.backUpTestId;
            newQuestion.Position = _context.Questions.Where(x => x.TestId == Consts.backUpTestId).Count();
            newQuestion.Points = 1;

            _context.Add(newQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = Consts.backUpTestId });
        }

        public async Task<IActionResult> AddChoice(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var newChoice = new Choice();
            newChoice.QuestionId = id;
            _context.Add(newChoice);
            await _context.SaveChangesAsync();

            var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id);
            return RedirectToAction("Edit", new { id = Consts.backUpTestId });
        }

        public async Task<IActionResult> DeleteQuestion(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
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
            return RedirectToAction("Edit", new { id = Consts.backUpTestId });
        }

        public async Task<IActionResult> DeleteChoice(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var oldChoice = await _context.Choices.FindAsync(id);
            _context.Remove(oldChoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = Consts.backUpTestId });
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