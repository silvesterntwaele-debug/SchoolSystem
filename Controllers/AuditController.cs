using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AuditController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AuditController(ApplicationDbContext context)
		{
			_context = context;
		}

		//=========================================
		// INDEX
		//=========================================
		public async Task<IActionResult> Index()
		{
			var audits = await _context.Audits
				.OrderByDescending(a => a.Timestamp)
				.ToListAsync();

			return View(audits);
		}

		//=========================================
		// DETAILS
		//=========================================
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var audit = await _context.Audits
				.FirstOrDefaultAsync(a => a.AuditId == id);

			if (audit == null)
				return NotFound();

			return View(audit);
		}
	}
}