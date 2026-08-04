using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer,Student")]
    public class ExamTimetablesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamTimetablesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var exams = _context.ExamTimetables
                .Include(e => e.Module)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                exams = exams.Where(e =>

                    (e.Module != null &&
                     e.Module.Code.Contains(searchString))

                    ||

                    (e.Venue != null &&
                     e.Venue.Contains(searchString))

                    ||

                    e.ExamDate.ToString().Contains(searchString)

                );
            }

            return View(await exams
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.ExamTimetables
                .Include(e => e.Module)
                .FirstOrDefaultAsync(e => e.ExamTimetableId == id);

            if (exam == null)
                return NotFound();

            return View(exam);
        }

        // =========================
        // CREATE
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ModuleId"] = new SelectList(
                _context.Modules,
                "ModuleId",
                "Code");

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamTimetable examTimetable)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ModuleId"] = new SelectList(
                    _context.Modules,
                    "ModuleId",
                    "Code",
                    examTimetable.ModuleId);

                return View(examTimetable);
            }

            _context.ExamTimetables.Add(examTimetable);
            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Exam Timetable Created",
                EntityName = "Exam Timetable",
                EntityId = examTimetable.ExamTimetableId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Exam timetable created for module {examTimetable.ModuleId}.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.ExamTimetables.FindAsync(id);

            if (exam == null)
                return NotFound();

            ViewData["ModuleId"] = new SelectList(
                _context.Modules,
                "ModuleId",
                "Code",
                exam.ModuleId);

            return View(exam);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamTimetable examTimetable)
        {
            if (id != examTimetable.ExamTimetableId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ModuleId"] = new SelectList(
                    _context.Modules,
                    "ModuleId",
                    "Code",
                    examTimetable.ModuleId);

                return View(examTimetable);
            }

            try
            {
                _context.Update(examTimetable);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Exam Timetable Updated",
                    EntityName = "Exam Timetable",
                    EntityId = examTimetable.ExamTimetableId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Exam timetable updated for module {examTimetable.ModuleId}.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(examTimetable.ExamTimetableId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.ExamTimetables
                .Include(e => e.Module)
                .FirstOrDefaultAsync(e => e.ExamTimetableId == id);

            if (exam == null)
                return NotFound();

            return View(exam);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.ExamTimetables.FindAsync(id);

            if (exam != null)
            {
                _context.ExamTimetables.Remove(exam);

                _context.Audits.Add(new Audit
                {
                    Action = "Exam Timetable Deleted",
                    EntityName = "Exam Timetable",
                    EntityId = exam.ExamTimetableId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Exam timetable deleted for module {exam.ModuleId}.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EXISTS
        // =========================
        private bool ExamExists(int id)
        {
            return _context.ExamTimetables.Any(e => e.ExamTimetableId == id);
        }
    }
}