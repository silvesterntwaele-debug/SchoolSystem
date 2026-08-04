using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Lecturer,Student")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //====================================================
        // INDEX WITH SEARCH
        //====================================================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            //==================================================
            // ADMIN OR LECTURER
            //==================================================
            if (User.IsInRole("Admin") || User.IsInRole("Lecturer"))
            {
                var registrations = _context.Registrations
                    .Include(r => r.Student)
                        .ThenInclude(s => s.User)
                    .Include(r => r.Module)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = searchString.ToLower();

                    registrations = registrations.Where(r =>
                        r.Student.StudentNumber.ToLower().Contains(searchString) ||
                        r.Module.Code.ToLower().Contains(searchString) ||
                        r.Semester.ToLower().Contains(searchString));
                }

                registrations = registrations
                    .OrderBy(r => r.Student.StudentNumber)
                    .ThenBy(r => r.Module.Code);

                return View(await registrations.ToListAsync());
            }

            //==================================================
            // STUDENT
            //==================================================

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var studentRegistrations = _context.Registrations
                .Include(r => r.Student)
                .Include(r => r.Module)
                .Where(r => r.StudentId == student.StudentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();

                studentRegistrations = studentRegistrations.Where(r =>
                    r.Module.Code.ToLower().Contains(searchString) ||
                    r.Semester.ToLower().Contains(searchString));
            }

            studentRegistrations = studentRegistrations
                .OrderBy(r => r.Module.Code);

            return View(await studentRegistrations.ToListAsync());
        }

        //====================================================
        // DETAILS
        //====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var registration = await _context.Registrations
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Module)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null)
                return NotFound();

            // Students may only view their own registrations
            if (User.IsInRole("Student"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Challenge();

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student == null)
                    return NotFound();

                if (registration.StudentId != student.StudentId)
                    return Forbid();
            }

            return View(registration);
        }
        //====================================================
        // CREATE
        //====================================================

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new RegistrationViewModel
            {
                Modules = _context.Modules
                    .OrderBy(m => m.Code)
                    .ToList()
            };

            ViewBag.StudentId = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber");

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegistrationViewModel model)
        {
            ViewBag.StudentId = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber",
                model.StudentId);

            model.Modules = await _context.Modules
                .OrderBy(m => m.Code)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            if (model.SelectedModules == null || !model.SelectedModules.Any())
            {
                ModelState.AddModelError("", "Please select at least one module.");
                return View(model);
            }

            int registrationsCreated = 0;

            foreach (var moduleId in model.SelectedModules)
            {
                bool exists = await _context.Registrations.AnyAsync(r =>
                    r.StudentId == model.StudentId &&
                    r.ModuleId == moduleId &&
                    r.Semester == model.Semester);

                if (exists)
                    continue;

                var registration = new Registration
                {
                    StudentId = model.StudentId,
                    ModuleId = moduleId,
                    Semester = model.Semester,
                    RegisteredOn = DateTime.UtcNow
                };

                _context.Registrations.Add(registration);
                registrationsCreated++;
            }

            if (registrationsCreated == 0)
            {
                ModelState.AddModelError("", "The student is already registered for all selected modules.");
                return View(model);
            }

            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Registration Created",
                EntityName = "Registration",
                EntityId = 0,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"{registrationsCreated} module(s) registered for Student ID {model.StudentId}.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{registrationsCreated} module(s) registered successfully.";

            return RedirectToAction(nameof(Index));
        }

        //====================================================
        // EDIT
        //====================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
                return NotFound();

            ViewBag.StudentId = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber",
                registration.StudentId);

            ViewBag.ModuleId = new SelectList(
                _context.Modules.OrderBy(m => m.Code),
                "ModuleId",
                "Code",
                registration.ModuleId);

            return View(registration);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Registration registration)
        {
            if (id != registration.RegistrationId)
                return NotFound();

            ViewBag.StudentId = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber",
                registration.StudentId);

            ViewBag.ModuleId = new SelectList(
                _context.Modules.OrderBy(m => m.Code),
                "ModuleId",
                "Code",
                registration.ModuleId);

            if (!ModelState.IsValid)
                return View(registration);

            bool duplicate = await _context.Registrations.AnyAsync(r =>
                r.RegistrationId != registration.RegistrationId &&
                r.StudentId == registration.StudentId &&
                r.ModuleId == registration.ModuleId &&
                r.Semester == registration.Semester);

            if (duplicate)
            {
                ModelState.AddModelError("", "This registration already exists.");
                return View(registration);
            }

            try
            {
                _context.Update(registration);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Registration Updated",
                    EntityName = "Registration",
                    EntityId = registration.RegistrationId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Student {registration.StudentId} registration updated for Module {registration.ModuleId}.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistrationExists(registration.RegistrationId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        //====================================================
        // DELETE
        //====================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var registration = await _context.Registrations
                .Include(r => r.Student)
                .Include(r => r.Module)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null)
                return NotFound();

            return View(registration);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Student)
                .Include(r => r.Module)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration != null)
            {
                _context.Registrations.Remove(registration);

                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Registration Deleted",
                    EntityName = "Registration",
                    EntityId = registration.RegistrationId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Student {registration.Student?.StudentNumber} was removed from Module {registration.Module?.Code}.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
        
        //====================================================
        // DELETE
        //====================================================


        //====================================================
        // EXISTS
        //====================================================

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(e => e.RegistrationId == id);
        }
    }
}