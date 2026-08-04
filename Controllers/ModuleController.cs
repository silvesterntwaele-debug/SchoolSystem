using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //=========================================
        // INDEX
        //=========================================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var modules = _context.Modules
                .Include(m => m.Lecturer)
                .AsQueryable();

            // Search ONLY by Lecturer Staff Number
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                modules = modules.Where(m =>
                    m.Lecturer != null &&
                    m.Lecturer.StaffNumber.Contains(searchString));
            }

            return View(await modules.ToListAsync());
        }

        //=========================================
        // DETAILS
        //=========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var module = await _context.Modules
                .Include(m => m.Lecturer)
                .Include(m => m.ExamTimetable)
                .FirstOrDefaultAsync(m => m.ModuleId == id);

            if (module == null)
                return NotFound();

            return View(module);
        }

        //=========================================
        // CREATE
        //=========================================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["LecturerId"] = new SelectList(
                _context.Lecturers,
                "LecturerId",
                "StaffNumber");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Module module)
        {
            if (!ModelState.IsValid)
            {
                ViewData["LecturerId"] = new SelectList(
                    _context.Lecturers,
                    "LecturerId",
                    "StaffNumber",
                    module.LecturerId);

                return View(module);
            }

            _context.Modules.Add(module);
            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Module Created",
                EntityName = "Module",
                EntityId = module.ModuleId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Module {module.Code} - {module.Name} created.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Module created successfully.";

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // EDIT
        //=========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var module = await _context.Modules.FindAsync(id);

            if (module == null)
                return NotFound();

            ViewData["LecturerId"] = new SelectList(
                _context.Lecturers,
                "LecturerId",
                "StaffNumber",
                module.LecturerId);

            return View(module);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Module module)
        {
            if (id != module.ModuleId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["LecturerId"] = new SelectList(
                    _context.Lecturers,
                    "LecturerId",
                    "StaffNumber",
                    module.LecturerId);

                return View(module);
            }

            try
            {
                _context.Update(module);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Module Updated",
                    EntityName = "Module",
                    EntityId = module.ModuleId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Module {module.Code} updated.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Module updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModuleExists(module.ModuleId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // DELETE
        //=========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var module = await _context.Modules
                .Include(m => m.Lecturer)
                .FirstOrDefaultAsync(m => m.ModuleId == id);

            if (module == null)
                return NotFound();

            return View(module);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var module = await _context.Modules.FindAsync(id);

            if (module != null)
            {
                _context.Modules.Remove(module);

                _context.Audits.Add(new Audit
                {
                    Action = "Module Deleted",
                    EntityName = "Module",
                    EntityId = module.ModuleId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Module {module.Code} deleted.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Module deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        //=========================================
        // EXISTS
        //=========================================
        private bool ModuleExists(int id)
        {
            return _context.Modules.Any(e => e.ModuleId == id);
        }
    }
}