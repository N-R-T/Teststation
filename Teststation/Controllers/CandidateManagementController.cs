using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teststation.Models;
using Teststation.Models.ViewModels;
using Microsoft.Office.Interop.Word;
using Microsoft.AspNetCore.Authorization;

namespace Teststation.Controllers
{
    [Authorize(Roles = Consts.Admin)]
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
            var candidates = _userManager.GetUsersInRoleAsync(Consts.Candidate).Result.Where(x=>!x.IsDeleted);

            var candidateList = new List<CandidateListEntryViewModel>();
            foreach (var candidate in candidates)
            {                
                candidateList.Add(new CandidateListEntryViewModel
                {
                    User = candidate,
                    UserId = candidate.Id,
                    Name = candidate.UserName,
                });
            }
            return View(candidateList);
        }

        public IActionResult CandidateDetails(string id)
        {
            if (!_context.Users.Any(x=>x.Id == id))
            {
                return RedirectToAction("CandidateList");
            }
            var viewModel = new CandidateSessionViewModel(_context, id);           
            return View(viewModel);
        }

        public async Task<IActionResult> DeleteCandidate(string id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            user.UserName =
                user.NormalizedUserName =
                user.Email =
                user.NormalizedEmail =
                user.PasswordHash = null;
            user.IsDeleted = true;
            await _userManager.RemoveFromRolesAsync(user, new List<string>() { Consts.Admin, Consts.Candidate });
            _context.Users.Update(user);
            _context.SaveChanges();
            return RedirectToAction(nameof(CandidateList));
        }

        public async Task<IActionResult> DeleteSession(long testId, string userId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var session = _context.Sessions.FirstOrDefault(x => x.TestId == testId && x.CandidateId == user.Id);
            var mathAnswers = _context.MathAnswers.Where(x => x.CandidateId == user.Id && x.Question.TestId == testId);
            var multipleChoiceAnswers = _context.MultipleChoiceAnswers.Where(x => x.CandidateId == user.Id && x.Choice.Question.TestId == testId);
            var circuitAnswers = _context.CircuitAnswers.Where(x => x.CandidateId == user.Id && x.Resistor.CircuitPart.Question.TestId == testId);

            _context.MathAnswers.RemoveRange(mathAnswers);
            _context.MultipleChoiceAnswers.RemoveRange(multipleChoiceAnswers);
            _context.Sessions.Remove(session);
            _context.SaveChanges();
            return RedirectToAction("CandidateDetails", new { id = userId });
        }

        #region Word

        public IActionResult ExportResults(string id)
        {
            var userId = id;
            var viewModel = new CandidateSessionViewModel(_context, userId);

            object fileName = @"Beurteilung_Erprobung_"+ viewModel.User.UserName + ".docx";
            ApplicationClass app = new ApplicationClass();
            object missing = System.Reflection.Missing.Value;
            Document document = app.Application.Documents.Add();
            document.Paragraphs.Format.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;

            document.Paragraphs.SpaceAfter = 0;
            document.Paragraphs.SpaceBefore = 0;
            document.Paragraphs.SpaceAfterAuto = 0;
            document.Paragraphs.SpaceBeforeAuto = 0;
            AddFootRow(document, missing, viewModel.User.UserName);
            AddDateToDocument(document, missing);
            AddHeadText(document, missing, viewModel.User.UserName);
            AddTable(document, missing, viewModel, app);
            AddTextAfterTable(document, missing);
            AddQualificationList(document, missing);
            AddNameOfAdmin(document, missing);
           

            document.SaveAs2(ref fileName,
                ref missing, ref missing, ref missing, ref missing, ref missing,
                ref missing, ref missing, ref missing, ref missing, ref missing,
                ref missing, ref missing, ref missing, ref missing, ref missing);
            document.Close();

            return RedirectToAction("CandidateDetails", new { id = userId });
        }


        private void AddDateToDocument(Document document, object missing)
        {            
            Paragraph paragraph = StyleForLargeText(document, missing);
            paragraph.Range.Text = "Erprobung am " + DateTime.Now.ToString("dd.MM.yyyy")+"\n";            
        }
        private void AddHeadText(Document document, object missing, string userName)
        {
            Paragraph paragraph = StyleForNormalText(document, missing);
            paragraph.Range.Text =
                "\n" +
                "Name des Teilnehmers: [Vorname] [Nachname]\n" +
                "\n" +
                "[Anrede] [Nachname] brachte zu Beginn der Erprobung zum Ausdruck, dass er sich für den Beruf des Fachinformatikers, Anwendungsentwicklung interessiert.\n" +
                "\n" +
                "Folgende Inhalte wurden getestet:\n" +
                "\n";      
        }
        private void AddTable(Document document, object missing, CandidateSessionViewModel viewModel, ApplicationClass app)
        {
            Paragraph paragraph = StyleForNormalText(document, missing);            
            var table = paragraph.Range.Tables.Add(paragraph.Range, viewModel.Tests.Count + 3, 6);
            table.Columns[1].Width = app.Application.CentimetersToPoints(8.11f);
            table.Columns[2].Width = app.Application.CentimetersToPoints(1.43f);
            table.Columns[3].Width = app.Application.CentimetersToPoints(1.43f);
            table.Columns[4].Width = app.Application.CentimetersToPoints(1.43f);
            table.Columns[5].Width = app.Application.CentimetersToPoints(1.79f);
            table.Columns[6].Width = app.Application.CentimetersToPoints(2.99f);

            TableHeader(table);
            SetTableStyle(table, app);            
            TableTests(table, viewModel);
            TableFooter(table, viewModel);            
        }
        private void TableHeader(Table table)
        {
            table.Cell(1, 1).Range.Text = "Aufgaben";
            table.Cell(1, 1).Range.Bold = 1;
            table.Cell(1, 1).Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleNone;
            table.Cell(2, 1).Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;

            table.Cell(1, 2).Range.Text = "Tag";
            table.Cell(1, 2).Range.Bold = 1;
            table.Cell(1, 3).Merge(table.Cell(1, 4));
            table.Cell(1, 2).Merge(table.Cell(1, 3));

            table.Cell(2, 2).Range.Text = "1";
            table.Cell(2, 3).Range.Text = "2";
            table.Cell(2, 4).Range.Text = "3";

            table.Cell(1, 3).Range.Text = "Ergebnis";
            table.Cell(1, 3).Range.Bold = 1;
            table.Cell(1, 3).Merge(table.Cell(1, 4));

            table.Cell(2, 5).Range.Text = "in %";
            table.Cell(2, 6).Range.Text = "Note IHK";
        }

        private void SetTableStyle(Table table, ApplicationClass app)
        {
            table.Range.Bold = 0;
            table.Range.Font.Size = 12;
            table.Spacing = 0;

            for (int r = 0; r <= table.Rows.Count; r++)
            {
                for (int c = 0; c <= table.Columns.Count; c++)
                {
                    try
                    {
                        table.Cell(r, c).Range.Borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleSingle;
                        table.Cell(r, c).Range.Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;
                        table.Cell(r, c).Range.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                        table.Cell(r, c).Range.Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;

                        table.Cell(r, c).HeightRule = WdRowHeightRule.wdRowHeightAuto;
                        table.Cell(r, c).Range.Paragraphs.SpaceAfter = 0;
                        table.Cell(r, c).Range.Paragraphs.SpaceBefore = 0;
                        table.Cell(r, c).Range.Italic = 1;
                        table.Cell(r, c).Range.Font.Name = "Frutiger 45 light";
                        table.Cell(r, c).Range.Font.Size = 12;
                    }
                    catch { }
                }
            }

            for (int r = 1; r <= 2; r++)
            {
                for (int c = 1; c <= 6; c++)
                {
                    try
                    {
                        table.Cell(r, c).Range.Shading.BackgroundPatternColor = WdColor.wdColorGray05;
                    }
                    catch { }
                }
            }

            for (int c = 1; c <= 6; c++)
            {
                try
                {
                    table.Cell(1, c).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                    table.Cell(1, c).VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    table.Cell(1, c).HeightRule = WdRowHeightRule.wdRowHeightExactly;
                    table.Cell(1, c).Height = app.Application.CentimetersToPoints(1.03f);
                }
                catch { }
            }

            
        }
        private void TableTests(Table table, CandidateSessionViewModel viewModel)
        {
            var rowIndex = 3;
            foreach (var test in viewModel.Tests)
            {
                table.Cell(rowIndex, 1).Range.Text = test.Test.Topic;
                table.Cell(rowIndex, 5).Range.Text = test.Result.ToString("P");
                table.Cell(rowIndex, 5).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                table.Cell(rowIndex, 6).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                rowIndex++;
            }
        }
        private void TableFooter(Table table, CandidateSessionViewModel viewModel)
        {
            var rowIndex = table.Rows.Count;
            table.Cell(rowIndex, 5).Range.Text = viewModel.WholeResult;
            table.Cell(rowIndex, 6).Range.Text = "0,00";
            table.Cell(rowIndex, 5).Range.Shading.BackgroundPatternColor = WdColor.wdColorGray05;
            table.Cell(rowIndex, 6).Range.Shading.BackgroundPatternColor = WdColor.wdColorGray05;
            table.Cell(rowIndex, 5).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(rowIndex, 6).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        }
        private void AddTextAfterTable(Document document, object missing)
        {
            Paragraph paragraph = StyleForNormalText(document, missing);
            paragraph.Range.Text =
                "\n" +
                "In wenigen Sätzen soll hier einerseits die Bewertung der oben erzielten Ergebnisse stehen. Grenzwert ist kumuliert 80% bzw. Punkte, d.h. diese messbare Größe muss erreicht werden um die fachlichen Anforderungen zu erfüllen. Im Einzelfall kann eine Zusage auch mit wenigen gemacht werden. Andererseits und entscheidend neben den messbaren Ergebnissen ist das Auftreten des Teilnehmer*in. Nur beides in Kombination, also gute messbare Ergebnisse und ein positiver Gesamteindruck während der gesamten Dauer der Erprobung entscheiden über die Zusage. Hinsichtlich der sozialen Kompetenzen sind folgend hier zu beurteilen:" +
                "\n" + 
                "\n";
        }
        private void AddQualificationList(Document document, object missing)
        {
            Paragraph paragraph = StyleForList(document, missing);
            string[] bulletItems = new string[] {
            "Motivation, Eigeninitiative, Ausdauer",
            "Pünktlichkeit und Verlässlichkeit",
            "Kommunikationskompetenz und Verhalten",
            "Kritikfähigkeit",
            "Transferfähigkeit, Problemlösendes Denken"};

            for (int i = 0; i < bulletItems.Length; i++)
            {
                string bulletItem = bulletItems[i];
                if (i < bulletItems.Length - 1)
                    bulletItem = bulletItem + "\n";
                paragraph.Range.InsertBefore(bulletItem);
            }
        }
        private void AddNameOfAdmin(Document document, object missing)
        {
            Paragraph paragraph = StyleForNormalText(document, missing);
            paragraph.Range.Text =
                "\n" +
                "\n" +
                "Name, Vorname\n" +
                "Ausbilder*in IT\t\n" +
                ""+
                "\n";
        }


        private void AddFootRow(Document document, object missing, string userName)
        {            
            Section section = document.Sections[1];
            Range footer = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
            footer.Font.Size = 8;
            footer.Font.Name = "Frutiger 45 light";
            footer.Text = "IT-Erprobung\t\tSeite 1\n" +
                "[Vorname] [Nachname]\n" +
                DateTime.Now.ToString("dd. MMMM yyyy");

        }

        private Paragraph StyleForLargeText(Document document, object missing)
        {
            Paragraph paragraph = StandardStyle(document, missing);
            paragraph.Range.Font.Bold = 1;
            paragraph.Range.Font.Size = 16;
            return paragraph;
        }
        private Paragraph StyleForNormalText(Document document, object missing)
        {
            Paragraph paragraph = StandardStyle(document, missing);            
            return paragraph;
        }
        private Paragraph StyleForList(Document document, object missing)
        {
            Paragraph paragraph = StandardStyle(document, missing);
            paragraph.Range.ListFormat.ApplyBulletDefault();
            paragraph.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            return paragraph;
        }
        private Paragraph StandardStyle(Document document, object missing)
        {
            Paragraph paragraph = document.Content.Paragraphs.Add(ref missing);
            paragraph.Range.Font.Bold = 0;
            paragraph.Range.Font.Name = "Frutiger 45 light";
            paragraph.Range.Font.Size = 12;
            return paragraph;
        }
        #endregion
    }
}