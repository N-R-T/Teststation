using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;
using User = Microsoft.AspNetCore.Identity.IdentityUser;

namespace Teststation.Controllers
{
    public class TestCreationController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signManager;
        private readonly Database _context;
        private static long originalTestId;
        private static string lastChangedQuestion;

        public TestCreationController(Database context, UserManager<User> userManager, SignInManager<User> signManager)
        {
            _userManager = userManager;
            _signManager = signManager;
            _context = context;
        }

        public async Task<IActionResult> Index(long? errorTestId)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var viewModel = new List<TestEntryViewModel>();
            var tests = await _context.Tests.ToListAsync();
            foreach (var testItem in tests)
            {
                var entry = new TestEntryViewModel { Test = testItem };
                if (testItem.Id == errorTestId)
                {
                    entry.Errors = ErrorsOfTest(testItem);
                }
                viewModel.Add(entry);
            }
            return View(viewModel);
        }

        #region Test kopieren

        public async Task<IActionResult> CopyTest(long? id)
        {
            var originalTest = GetOriginalTest(id);

            var copiedTest = new Test();
            copiedTest.Topic = CopiedTestName(originalTest.Topic);
            _context.Tests.Add(copiedTest);
            _context.SaveChanges();
            copiedTest = _context.Tests.FirstOrDefault(x => x.Id == copiedTest.Id);

            if (originalTest.Questions != null)
            {
                CopyMathQuestions(originalTest, copiedTest.Id);
                CopyMultipleChoiceQuestions(originalTest, copiedTest.Id);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private void CopyMathQuestions(Test originalTest, long copiedTestId)
        {
            foreach (var originalQuestion in originalTest.Questions.Where(x => x is MathQuestion).Select(x => x as MathQuestion))
            {
                var copiedQuestion = new MathQuestion
                {
                    CorrectAnswer = originalQuestion.CorrectAnswer,
                    Points = originalQuestion.Points,
                    Position = originalQuestion.Position,
                    TestId = copiedTestId,
                    Text = originalQuestion.Text,
                };
                _context.Add(copiedQuestion);
            }
        }

        private void CopyMultipleChoiceQuestions(Test originalTest, long copiedTestId)
        {
            foreach (var originalQuestion in originalTest.Questions.Where(x => x is MultipleChoiceQuestion).Select(x => x as MultipleChoiceQuestion))
            {
                var copiedQuestion = new MultipleChoiceQuestion
                {
                    Points = originalQuestion.Points,
                    Position = originalQuestion.Position,
                    TestId = copiedTestId,
                    Text = originalQuestion.Text,
                };
                _context.Add(copiedQuestion);
                _context.SaveChanges();

                foreach (var originalChoice in originalQuestion.Choices)
                {
                    var copiedChoice = new Choice
                    {
                        Correct = originalChoice.Correct,
                        QuestionId = copiedQuestion.Id,
                        Text = originalChoice.Text,
                    };
                    _context.Add(copiedChoice);
                }
            }
        }

        private Test GetOriginalTest(long? id)
        {
            var originalTest = _context.Tests.FirstOrDefault(x => x.Id == id);
            originalTest.Questions = _context.Questions.Where(x => x.TestId == originalTest.Id).ToList();
            foreach (var question in originalTest.Questions
                .Where(x=>x is MultipleChoiceQuestion)
                .Select(x=>x as MultipleChoiceQuestion))
            {
                question.Choices = _context.Choices.Where(x=>x.QuestionId == question.Id).ToList();
            }

            return originalTest;
        }

        private string CopiedTestName(string originalName)
        {
            var newName = "Kopie von " + originalName;
            var existingNames = _context.Tests.Select(x => x.Topic);
            var counter = 1;
            while (existingNames.Any(x => x == newName))
            {
                newName = "Kopie von " + originalName + " (" + counter + ")";
                counter++;
            }
            return newName;
        }
        #endregion

        #region Neuen Test erstellen
        public async Task<IActionResult> Create()
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var test = new Test { Topic = Consts.fillerNameForNewTest };
            _context.Add(test);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { test.Id });
        }
        #endregion

        #region Test bearbeiten
        public async Task<IActionResult> Edit(long? id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            if (_context.Tests.Any(x => x.Id == id))
            {
                if (_context.Tests.FirstOrDefault(x => x.Id == id).ReleaseStatus != TestStatus.InProgress)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (id != Consts.backUpTestId)
            {
                return RedirectToAction(nameof(Index));
            }

            if (id != Consts.backUpTestId)
            {
                originalTestId = (long)id;
                return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, lastChangedQuestion);
            }

            var backUpTest = CreateBackUp();
            var viewModel = TestCreationTransformer.TransformToTestCreationViewModel(backUpTest);
            viewModel.LastChangedQuestion = lastChangedQuestion;            
            return View(viewModel);
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
            try
            {
                SaveTest(test);
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
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
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

            foreach (var question in test.Questions
                .Where(x => x is MultipleChoiceQuestion)
                .Select(x => x as MultipleChoiceQuestion)
                .ToList())
            {
                question.TestId = test.Id;
                if (question.Choices != null)
                {
                    foreach (var choice in question.Choices)
                    {
                        choice.QuestionId = question.Id;
                    }
                }
            }

            foreach (var question in test.Questions
                .Where(x => x is MathQuestion)
                .Select(x => x as MathQuestion)
                .ToList())
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
                backUpTest = new Test(test)
                {
                    Questions = _context.Questions.Where(x => x.TestId == test.Id).ToList()
                };
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
                _context.Questions.RemoveRange(_context.Questions.Where(x => x.TestId == Consts.backUpTestId));
                _context.Choices.RemoveRange(_context.Choices
                    .Include(x => x.Question)
                    .Where(x => x.Question.TestId == Consts.backUpTestId));
                _context.Tests.Remove(_context.Tests.FirstOrDefault(x => x.Id == Consts.backUpTestId));
                _context.SaveChanges();
            }
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
                if (question.Choices != null)
                {
                    foreach (var choice in question.Choices)
                    {
                        _context.Update(choice);
                    }
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

            return RedirectToAction("Edit", "TestCreation", new { id }, newQuestion.Id.ToString());
        }
        public async Task<IActionResult> AddMultipleChoiceQuestion(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var newQuestion = new MultipleChoiceQuestion
            {
                TestId = Consts.backUpTestId,
                Position = _context.Questions.Where(x => x.TestId == Consts.backUpTestId).Count(),
                Points = 1
            };

            _context.Add(newQuestion);
            await _context.SaveChangesAsync();

            newQuestion = _context.Questions.FirstOrDefault(x => x.Id == newQuestion.Id) as MultipleChoiceQuestion;
            _context.Add(new Choice
            {
                QuestionId = newQuestion.Id
            });
            _context.Add(new Choice
            {
                QuestionId = newQuestion.Id
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "TestCreation", new { id }, newQuestion.Id.ToString());
        }

        public async Task<IActionResult> AddChoice(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var newChoice = new Choice();
            newChoice.QuestionId = id;
            _context.Add(newChoice);
            await _context.SaveChangesAsync();

            var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id);

            return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, id.ToString());
        }

        public async Task<IActionResult> PushQuestionUp(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var question = await _context.Questions.FindAsync(id);
            var otherQuestion = await _context.Questions.FirstOrDefaultAsync(x => x.Position == question.Position - 1 && x.TestId == question.TestId);
            question.Position--;
            otherQuestion.Position++;
            _context.Update(question);
            _context.Update(otherQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, id.ToString());
        }

        public async Task<IActionResult> PushQuestionDown(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            var question = await _context.Questions.FindAsync(id);
            var otherQuestion = await _context.Questions.FirstOrDefaultAsync(x => x.Position == question.Position + 1 && x.TestId == question.TestId);
            question.Position++;
            otherQuestion.Position--;
            _context.Update(question);
            _context.Update(otherQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, id.ToString());
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
            return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, id.ToString());
        }

        public async Task<IActionResult> DeleteChoice(long id, [Bind("Id,Topic,Questions")] TestCreationViewModel model)
        {
            SaveTest(TestCreationTransformer.TransformToTest(model));
            
            var oldChoice = await _context.Choices.FindAsync(id);
            var question = await _context.Questions.FindAsync(oldChoice.QuestionId);
            _context.Remove(oldChoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "TestCreation", new { id = Consts.backUpTestId }, question.Id.ToString());
        }
        #endregion

        #region Löschen
        public async Task<IActionResult> Delete(long? id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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
            var mathQuestions = _context.MathQuestions
                .Where(x => x.TestId == test.Id)
                .ToList();
            var multipleChoiceQuestions = _context.MultipleChoiceQuestions
                .Where(x => x.TestId == test.Id)
                .Include(x => x.Choices)
                .ToList();
            var mathAnswers = _context.MathAnswers
                .Where(x => x.Question.TestId == test.Id)
                .ToList();
            var multipleChoiceAnswers = _context.MultipleChoiceAnswers
                .Where(x => x.Choice.Question.TestId == test.Id)
                .ToList();

            if (mathQuestions.Count != 0)
            {
                _context.MathQuestions.RemoveRange(mathQuestions);
            }

            if (multipleChoiceQuestions.Count != 0)
            {
                _context.MultipleChoiceQuestions.RemoveRange(multipleChoiceQuestions);
            }

            if (mathAnswers.Count != 0)
            {
                _context.MathAnswers.RemoveRange(mathAnswers);
            }

            if (multipleChoiceAnswers.Count != 0)
            {
                _context.MultipleChoiceAnswers.RemoveRange(multipleChoiceAnswers);
            }

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

        public async Task<IActionResult> ReleaseTest(long? id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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

            if (ErrorsOfTest(test).Count > 0)
            {
                return RedirectToAction("Index", new { errorTestId = test.Id });
            }
            test.ReleaseStatus = TestStatus.Public;
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CloseTest(long? id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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
            test.ReleaseStatus = TestStatus.Closed;
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Regress(long? id)
        {
            if (IsNotAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
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
            test.ReleaseStatus = TestStatus.InProgress;
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private List<string> ErrorsOfTest(Test test)
        {
            var errors = new List<string>();
            if (_context.Tests.Any(x => x.Id != test.Id && x.Topic == test.Topic && x.ReleaseStatus == TestStatus.Public))
            {
                errors.Add("Ungültiges Themengebiet!");//Ein öffentlicher Test mit dem gleichen Namen existiert schon.
            }
            if (test.Topic == Consts.fillerNameForNewTest)
            {
                errors.Add("Ungültiges Themengebiet!");//Der Name wurde nicht verändert.
            }
            if (test.Topic == null)
            {
                errors.Add("Ungültiges Themengebiet!");//Der Name ist leer.
            }

            var allQuestions = _context.Questions
                .Where(x => x.TestId == test.Id)
                .ToList();

            if (allQuestions.Count == 0)
            {
                errors.Add("Der Test hat keine Fragen!");//Der Test hat keine Fragen.
            }
            if (allQuestions.Any(x => x.Text == null))
            {
                errors.Add("Eine Frage hat einen Fehler!");//Die Frage hat keinen Text.
            }
            if (allQuestions.Any(x => x.Points == 0))
            {
                errors.Add("Eine Frage hat einen Fehler!");//Die Frage gibt keine Punkte.
            }

            var mathQuestions = _context.MathQuestions
                .Where(x => x.TestId == test.Id)
                .ToList();

            if (mathQuestions.Any(x => x.CorrectAnswer == null))
            {
                errors.Add("Eine Frage hat keine richtige Antwort!");//Die Frage hat keine richtige Antwort.
            }

            var multipleChoiceQuestions = _context.MultipleChoiceQuestions
                .Where(x => x.TestId == test.Id)
                .Include(x => x.Choices)
                .ToList();

            if (multipleChoiceQuestions.Any(x => x.Choices.Count == 0))
            {
                errors.Add("Eine Frage hat keine Antwortmöglichkeiten!");//Die Frage hat keine Antwortmöglichkeiten.
            }

            if (multipleChoiceQuestions.Any(x => x.Choices.Any(y => y.Text == null)))
            {
                errors.Add("Eine Antwortmöglichkeit hat keinen Text!");//Die Antwortmöglichkeit hat keinen Text.
            }

            if (multipleChoiceQuestions.Any(x => x.Choices.All(y => !y.Correct)))
            {
                errors.Add("Eine Frage hat keine richtige Antwortmöglichkeit!");//Die Frage hat 0 richtige Antwortmöglichkeit.
            }

            return errors;
        }

        private bool IsNotAdmin()
        {
            if (!_signManager.IsSignedIn(User))
            {
                return true;
            }
            if (_context.UserInformation.Any(x => x.UserId == _userManager.GetUserId(User)))
            {
                return _context.UserInformation.First(x => x.UserId == _userManager.GetUserId(User)).Role != UserRole.Admin;
            }
            return false;
        }
    }
}