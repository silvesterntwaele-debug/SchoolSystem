using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AdminController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> AdminDashboard()
		{
			ViewBag.StudentCount = await _context.Students.CountAsync();
			ViewBag.LecturerCount = await _context.Lecturers.CountAsync();
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

