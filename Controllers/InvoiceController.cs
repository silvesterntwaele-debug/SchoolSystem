using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin,Student")]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoicesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //====================================================
        // INDEX
        //====================================================
        public async Task<IActionResult> Index()
        {
            // Admin sees every invoice
            if (User.IsInRole("Admin"))
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Student)
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                return View(invoices);
            }

            // Student only sees their own invoices
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var invoicesList = await _context.Invoices
                .Include(i => i.Student)
                .Where(i => i.StudentId == student.StudentId)
                .OrderByDescending(i => i.CreatedOn)
                .ToListAsync();

            return View(invoicesList);
        }

        //====================================================
        // DETAILS
        //====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Student)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            // Student can only open their own invoice
            if (User.IsInRole("Student"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Challenge();

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student == null)
                    return NotFound();

                if (invoice.StudentId != student.StudentId)
                    return Forbid();
            }

            return View(invoice);
        }
        //====================================================
        // CREATE
        //====================================================

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(
                _context.Students
                    .OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] = new SelectList(
                    _context.Students.OrderBy(s => s.StudentNumber),
                    "StudentId",
                    "StudentNumber",
                    invoice.StudentId);

                return View(invoice);
            }

            invoice.CreatedOn = DateTime.UtcNow;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Invoice Created",
                EntityName = "Invoice",
                EntityId = invoice.InvoiceId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Invoice created for Student {invoice.StudentId}.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

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

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
                return NotFound();

            ViewData["StudentId"] = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber",
                invoice.StudentId);

            return View(invoice);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] = new SelectList(
                    _context.Students.OrderBy(s => s.StudentNumber),
                    "StudentId",
                    "StudentNumber",
                    invoice.StudentId);

                return View(invoice);
            }

            try
            {
                _context.Update(invoice);
                await _context.SaveChangesAsync();

                _context.Audits.Add(new Audit
                {
                    Action = "Invoice Updated",
                    EntityName = "Invoice",
                    EntityId = invoice.InvoiceId,
                    PerformedByUserId = User.Identity?.Name ?? "System",
                    Details = $"Invoice #{invoice.InvoiceId} updated.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(invoice.InvoiceId))
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

            var invoice = await _context.Invoices
                .Include(i => i.Student)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Student)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Invoice Deleted",
                EntityName = "Invoice",
                EntityId = invoice.InvoiceId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Invoice #{invoice.InvoiceId} for student {invoice.Student.StudentNumber} was deleted.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //====================================================
        // EXISTS
        //====================================================

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}