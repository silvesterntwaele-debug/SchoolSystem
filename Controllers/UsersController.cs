using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        //====================================================
        // INDEX
        //====================================================

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user))
                    .FirstOrDefault();

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserID == user.Id);

                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Role = role,
                    EmailConfirmed = user.EmailConfirmed,
                    StudentNumber = student?.StudentNumber,
                    StaffNumber = lecturer?.StaffNumber
                });
            }

            return View(model);
        }
        //====================================================
        // DETAILS
        //====================================================

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserID == user.Id);

            var model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                StudentNumber = student?.StudentNumber,
                StaffNumber = lecturer?.StaffNumber
            };

            return View(model);
        }
        //====================================================
        // DELETE
        //====================================================

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserID == user.Id);

            var model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                StudentNumber = student?.StudentNumber,
                StaffNumber = lecturer?.StaffNumber
            };

            return View(model);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(Index));

            // Prevent admin from deleting themselves
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Remove linked Student record
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student != null)
                _context.Students.Remove(student);

            // Remove linked Lecturer record
            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserID == user.Id);

            if (lecturer != null)
                _context.Lecturers.Remove(lecturer);

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _context.Audits.Add(new Audit
                {
                    Action = "User Deleted",
                    EntityName = "ApplicationUser",
                    EntityId = 0,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Deleted user {user.UserName}",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
        //====================================================
        // EDIT
        //====================================================

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserID == user.Id);

            var model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                StudentNumber = student?.StudentNumber,
                StaffNumber = lecturer?.StaffNumber
            };

            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles
                    .Select(r => r.Name)
                    .ToList();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                ViewBag.Roles = _roleManager.Roles
                    .Select(r => r.Name)
                    .ToList();

                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            _context.Audits.Add(new Audit
            {
                Action = "User Updated",
                EntityName = "ApplicationUser",
                EntityId = 0,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Updated user {user.UserName}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

