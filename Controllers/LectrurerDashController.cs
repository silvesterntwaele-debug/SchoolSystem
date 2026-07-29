using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> LecturerDashboard()
        {
            ViewBag.ModuleCount = await _context.Modules.CountAsync();
            ViewBag.StudentCount = await _context.Students.CountAsync();
            ViewBag.StudentModuleMarkSheetCount = _context.StudentModuleMarkSheets.Count();
            ViewBag.ExamCount = await _context.ExamTimetables.CountAsync();

            return View();
        }
    }
}