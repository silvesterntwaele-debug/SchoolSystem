using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> StudentDashboard()
        {
            ViewBag.ModuleCount = await _context.Modules.CountAsync();
            ViewBag.RegistrationCount = await _context.Registrations.CountAsync();
            ViewBag.StudentModuleMarkSheetCount = _context.StudentModuleMarkSheets.Count();
            ViewBag.InvoiceCount = await _context.Invoices.CountAsync();
            ViewBag.PaymentCount = await _context.Payments.CountAsync();
            ViewBag.ExamCount = await _context.ExamTimetables.CountAsync();

            return View();
        }
    }
}