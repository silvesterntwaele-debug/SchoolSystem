using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Index()
        {
            var lecturers = _context.Lecturers
                .Include(l => l.User);

            return View(await lecturers.ToListAsync());
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
		public IActionResult Create()
		{
			ViewBag.UserID = new SelectList(_context.Users, "Id", "Email");
			return View();
		}

		// =========================
		// CREATE (POST)
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Lecturer lecturer)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.UserID = new SelectList(_context.Users, "Id", "Email", lecturer.UserID);
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

				ViewBag.UserID = new SelectList(_context.Users, "Id", "Email", lecturer.UserID);

				return View(lecturer);
			}
		}


		// =========================
		// EDIT
		// =========================
		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var lecturer = await _context.Lecturers.FindAsync(id);

            if (lecturer == null)
                return NotFound();

            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Email", lecturer.UserID);

            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lecturer lecturer)
        {
            if (id != lecturer.LecturerId)
                return NotFound();

            if (ModelState.IsValid)
            {
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LecturerExists(lecturer.LecturerId))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Email", lecturer.UserID);

            return View(lecturer);
        }

        // =========================
        // DELETE
        // =========================
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