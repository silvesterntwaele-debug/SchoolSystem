using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;
using System.Security.Claims;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer,Student")]
    public class StudentModuleMarkSheetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentModuleMarkSheetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //=========================================
        // CALCULATE FINAL MARK
        //=========================================

        private decimal CalculateFinalMark(StudentModuleMarkSheet sheet)
        {
            var marks = new List<decimal>();

            if (sheet.Test1.HasValue) marks.Add(sheet.Test1.Value);
            if (sheet.Test2.HasValue) marks.Add(sheet.Test2.Value);
            if (sheet.Test3.HasValue) marks.Add(sheet.Test3.Value);

            if (sheet.Assignment1.HasValue) marks.Add(sheet.Assignment1.Value);
            if (sheet.Assignment2.HasValue) marks.Add(sheet.Assignment2.Value);

            if (sheet.Practical.HasValue) marks.Add(sheet.Practical.Value);

            if (sheet.Project.HasValue) marks.Add(sheet.Project.Value);

            decimal classAverage = marks.Any() ? marks.Average() : 0;

            decimal exam = sheet.Exam ?? 0;

            return Math.Round((classAverage * 0.40m) + (exam * 0.60m), 2);
        }

        //=========================================
        // INDEX
        //=========================================

        public async Task<IActionResult> Index()
        {
            IQueryable<StudentModuleMarkSheet> marksheets =
                _context.StudentModuleMarkSheets
                .Include(s => s.Student)
                .Include(s => s.Module);

            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                marksheets = marksheets.Where(m =>
                    m.Student != null &&
                    m.Student.UserId == userId);
            }

            return View(await marksheets.ToListAsync());
        }

        //=========================================
        // DETAILS
        //=========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var sheet = await _context.StudentModuleMarkSheets
                .Include(s => s.Student)
                .Include(s => s.Module)
                .FirstOrDefaultAsync(s => s.StudentModuleMarkSheetId == id);

            if (sheet == null)
                return NotFound();

            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (sheet.Student?.UserId != userId)
                    return Forbid();
            }

            return View(sheet);
        }

        //=========================================
        // CREATE
        //=========================================

        [Authorize(Roles = "Admin,Lecturer")]
        public IActionResult Create()
        {
            ViewData["StudentId"] =
                new SelectList(_context.Students,
                "StudentId",
                "StudentNumber");

            ViewData["ModuleId"] =
                new SelectList(_context.Modules,
                "ModuleId",
                "Code");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> Create(StudentModuleMarkSheet sheet)
        {
            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] =
                    new SelectList(_context.Students,
                    "StudentId",
                    "StudentNumber",
                    sheet.StudentId);

                ViewData["ModuleId"] =
                    new SelectList(_context.Modules,
                    "ModuleId",
                    "Code",
                    sheet.ModuleId);

                return View(sheet);
            }

            bool exists = await _context.StudentModuleMarkSheets.AnyAsync(s =>
                s.StudentId == sheet.StudentId &&
                s.ModuleId == sheet.ModuleId);

            if (exists)
            {
                ModelState.AddModelError("", "This student already has a marksheet for this module.");

                ViewData["StudentId"] =
                    new SelectList(_context.Students,
                    "StudentId",
                    "StudentNumber",
                    sheet.StudentId);

                ViewData["ModuleId"] =
                    new SelectList(_context.Modules,
                    "ModuleId",
                    "Code",
                    sheet.ModuleId);

                return View(sheet);
            }

            sheet.FinalMark = CalculateFinalMark(sheet);
            sheet.LastUpdated = DateTime.UtcNow;

            _context.StudentModuleMarkSheets.Add(sheet);

            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Student Module MarkSheet Created",
                EntityName = "StudentModuleMarkSheet",
                EntityId = sheet.StudentModuleMarkSheetId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Created marksheet for Student {sheet.StudentId} Module {sheet.ModuleId}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        //=========================================
        // EDIT
        //=========================================

        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var sheet = await _context.StudentModuleMarkSheets.FindAsync(id);

            if (sheet == null)
                return NotFound();

            ViewData["StudentId"] =
                new SelectList(_context.Students,
                "StudentId",
                "StudentNumber",
                sheet.StudentId);

            ViewData["ModuleId"] =
                new SelectList(_context.Modules,
                "ModuleId",
                "Code",
                sheet.ModuleId);

            return View(sheet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> Edit(int id, StudentModuleMarkSheet sheet)
        {
            if (id != sheet.StudentModuleMarkSheetId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] =
                    new SelectList(_context.Students,
                    "StudentId",
                    "StudentNumber",
                    sheet.StudentId);

                ViewData["ModuleId"] =
                    new SelectList(_context.Modules,
                    "ModuleId",
                    "Code",
                    sheet.ModuleId);

                return View(sheet);
            }

            try
            {
                sheet.FinalMark = CalculateFinalMark(sheet);
                sheet.LastUpdated = DateTime.UtcNow;

                _context.Update(sheet);

                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Student Module MarkSheet Updated",
                    EntityName = "StudentModuleMarkSheet",
                    EntityId = sheet.StudentModuleMarkSheetId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Updated marksheet for Student {sheet.StudentId} Module {sheet.ModuleId}",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentModuleMarkSheetExists(sheet.StudentModuleMarkSheetId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // DELETE
        //=========================================

        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var sheet = await _context.StudentModuleMarkSheets
                .Include(s => s.Student)
                .Include(s => s.Module)
                .FirstOrDefaultAsync(s => s.StudentModuleMarkSheetId == id);

            if (sheet == null)
                return NotFound();

            return View(sheet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sheet = await _context.StudentModuleMarkSheets.FindAsync(id);

            if (sheet != null)
            {
                _context.StudentModuleMarkSheets.Remove(sheet);

                _context.Audits.Add(new Audit
                {
                    Action = "Student Module MarkSheet Deleted",
                    EntityName = "StudentModuleMarkSheet",
                    EntityId = sheet.StudentModuleMarkSheetId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Deleted marksheet for Student {sheet.StudentId} Module {sheet.ModuleId}",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // EXISTS
        //=========================================

        private bool StudentModuleMarkSheetExists(int id)
        {
            return _context.StudentModuleMarkSheets
                .Any(e => e.StudentModuleMarkSheetId == id);
        }
    }
}