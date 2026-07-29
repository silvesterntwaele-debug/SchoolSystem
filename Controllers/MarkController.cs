using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class MarksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarksController(ApplicationDbContext context)
        {
            _context = context;
        }

        //=========================================
        // INDEX
        //=========================================

        public async Task<IActionResult> Index()
        {
            var marks = _context.Marks
                .Include(m => m.Student)
                .Include(m => m.Module);

            return View(await marks.ToListAsync());
        }

        //=========================================
        // DETAILS
        //=========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var mark = await _context.Marks
                .Include(m => m.Student)
                .Include(m => m.Module)
                .FirstOrDefaultAsync(m => m.MarkId == id);

            if (mark == null)
                return NotFound();

            return View(mark);
        }

        //=========================================
        // CREATE
        //=========================================

        public IActionResult Create()
        {
            ViewData["StudentId"] =
                new SelectList(_context.Students, "StudentId", "StudentNumber");

            ViewData["ModuleId"] =
                new SelectList(_context.Modules, "ModuleId", "Code");

            return View();
        }
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Mark mark)
		{
			if (!ModelState.IsValid)
			{
				ViewData["StudentId"] = new SelectList(
					_context.Students,
					"StudentId",
					"StudentNumber",
					mark.StudentId);

				ViewData["ModuleId"] = new SelectList(
					_context.Modules,
					"ModuleId",
					"Code",
					mark.ModuleId);

				return View(mark);
			}

			mark.DateRecorded = DateTime.UtcNow;
			mark.RecordedByUserId = User.Identity?.Name ?? "System";

			_context.Marks.Add(mark);
			await _context.SaveChangesAsync();

			_context.Audits.Add(new Audit
			{
				Action = "Mark Created",
				EntityName = "Mark",
				EntityId = mark.MarkId,
				PerformedByUserId = User.Identity?.Name ?? "System",
				Details = $"{mark.AssessmentType} mark of {mark.Score}% recorded.",
				Timestamp = DateTime.UtcNow
			});

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}


		//=========================================
		// EDIT
		//=========================================

		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var mark = await _context.Marks.FindAsync(id);

            if (mark == null)
                return NotFound();

            ViewData["StudentId"] =
                new SelectList(_context.Students, "StudentId", "StudentNumber", mark.StudentId);

            ViewData["ModuleId"] =
                new SelectList(_context.Modules, "ModuleId", "Code", mark.ModuleId);

            return View(mark);
        }
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Mark mark)
		{
			if (id != mark.MarkId)
				return NotFound();

			if (!ModelState.IsValid)
			{
				ViewData["StudentId"] = new SelectList(
					_context.Students,
					"StudentId",
					"StudentNumber",
					mark.StudentId);

				ViewData["ModuleId"] = new SelectList(
					_context.Modules,
					"ModuleId",
					"Code",
					mark.ModuleId);

				return View(mark);
			}

			try
			{
				mark.LastEditedOn = DateTime.UtcNow;

				_context.Update(mark);
				await _context.SaveChangesAsync();

				_context.Audits.Add(new Audit
				{
					Action = "Mark Updated",
					EntityName = "Mark",
					EntityId = mark.MarkId,
					PerformedByUserId = User.Identity?.Name ?? "System",
					Details = $"Mark updated to {mark.Score}%.",
					Timestamp = DateTime.UtcNow
				});

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!MarkExists(mark.MarkId))
					return NotFound();

				throw;
			}

			return RedirectToAction(nameof(Index));
		}


		//=========================================
		// DELETE
		//=========================================

		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var mark = await _context.Marks
                .Include(m => m.Student)
                .Include(m => m.Module)
                .FirstOrDefaultAsync(m => m.MarkId == id);

            if (mark == null)
                return NotFound();

            return View(mark);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mark = await _context.Marks.FindAsync(id);

            if (mark != null)
            {
                _context.Marks.Remove(mark);

                _context.Audits.Add(new Audit
                {
                    Action = "Mark Deleted",
                    EntityName = "Mark",
                    EntityId = mark.MarkId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Mark of {mark.Score}% deleted.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // EXISTS
        //=========================================

        private bool MarkExists(int id)
        {
            return _context.Marks.Any(e => e.MarkId == id);
        }
    }
}
