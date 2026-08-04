using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var students = _context.Students
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                students = students.Where(s =>
                    s.StudentNumber.Contains(searchString));
            }

            students = students
                .OrderBy(s => s.StudentNumber);

            return View(await students.ToListAsync());
        }


        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["UserId"] =
                new SelectList(_context.Users, "Id", "Email");

            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewData["UserId"] =
                    new SelectList(_context.Users, "Id", "Email", student.UserId);

                return View(student);
            }

            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Student Created",
                    EntityName = "Student",
                    EntityId = student.StudentId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Student {student.StudentNumber} was created.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Student created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                ViewData["UserId"] =
                    new SelectList(_context.Users, "Id", "Email", student.UserId);

                return View(student);
            }
        }

        // =========================
        // EDIT (GET)
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                student.User = await _context.Users.FindAsync(student.UserId);
                return View(student);
            }

            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Student Updated",
                    EntityName = "Student",
                    EntityId = student.StudentId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Student {student.StudentNumber} was updated.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Student updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.StudentId))
                    return NotFound();

                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                student.User = await _context.Users.FindAsync(student.UserId);

                return View(student);
            }
        }

        // =========================
        // DELETE (GET)
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student != null)
            {
                _context.Students.Remove(student);

                _context.Audits.Add(new Audit
                {
                    Action = "Student Deleted",
                    EntityName = "Student",
                    EntityId = student.StudentId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Student {student.StudentNumber} was deleted.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EXISTS
        // =========================
        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}