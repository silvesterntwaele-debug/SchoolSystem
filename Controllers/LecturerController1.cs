using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class LecturersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var lecturers = _context.Lecturers
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                lecturers = lecturers.Where(l =>

                    (l.StaffNumber != null &&
                     l.StaffNumber.Contains(searchString))

                    ||

                    (l.Department != null &&
                     l.Department.Contains(searchString))

                    ||

                    (l.User != null &&
                     l.User.Email.Contains(searchString))
                );
            }

            return View(await lecturers
                .OrderBy(l => l.StaffNumber)
                .ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var lecturer = await _context.Lecturers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LecturerId == id);

            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.UserID = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserID = new SelectList(
                    _context.Users,
                    "Id",
                    "Email",
                    lecturer.UserID);

                return View(lecturer);
            }

            try
            {
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Lecturer Created",
                    EntityName = "Lecturer",
                    EntityId = lecturer.LecturerId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Lecturer {lecturer.StaffNumber} was created.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Lecturer created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                ViewBag.UserID = new SelectList(
                    _context.Users,
                    "Id",
                    "Email",
                    lecturer.UserID);

                return View(lecturer);
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

            var lecturer = await _context.Lecturers.FindAsync(id);

            if (lecturer == null)
                return NotFound();

            ViewData["UserID"] = new SelectList(
                _context.Users,
                "Id",
                "Email",
                lecturer.UserID);

            return View(lecturer);

        }

        // =========================
        // EDIT (POST)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lecturer lecturer)
        {
            if (id != lecturer.LecturerId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["UserID"] = new SelectList(
                    _context.Users,
                    "Id",
                    "Email",
                    lecturer.UserID);

                return View(lecturer);
            }

            try
            {
                _context.Update(lecturer);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Lecturer Updated",
                    EntityName = "Lecturer",
                    EntityId = lecturer.LecturerId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Lecturer {lecturer.StaffNumber} was updated.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Lecturer updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LecturerExists(lecturer.LecturerId))
                    return NotFound();

                throw;
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

            var lecturer = await _context.Lecturers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LecturerId == id);

            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);

            if (lecturer != null)
            {
                _context.Lecturers.Remove(lecturer);

                _context.Audits.Add(new Audit
                {
                    Action = "Lecturer Deleted",
                    EntityName = "Lecturer",
                    EntityId = lecturer.LecturerId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Lecturer {lecturer.StaffNumber} was deleted.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Lecturer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EXISTS
        // =========================
        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.LecturerId == id);
        }
    }
}
