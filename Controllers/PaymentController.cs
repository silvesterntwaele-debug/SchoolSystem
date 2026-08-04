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
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //====================================================
        // INDEX
        //====================================================
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            //====================================================
            // ADMIN
            //====================================================
            if (User.IsInRole("Admin"))
            {
                var payments = _context.Payments
                    .Include(p => p.Invoice)
                        .ThenInclude(i => i.Student)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    payments = payments.Where(p =>
                        p.Invoice.Student.StudentNumber.Contains(searchString) ||
                        p.InvoiceId.ToString().Contains(searchString));
                }

                payments = payments.OrderByDescending(p => p.PaidOn);

                return View(await payments.ToListAsync());
            }

            //====================================================
            // STUDENT
            //====================================================

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return NotFound();

            var studentPayments = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Student)
                .Where(p => p.Invoice.StudentId == student.StudentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                studentPayments = studentPayments.Where(p =>
                    p.InvoiceId.ToString().Contains(searchString));
            }

            studentPayments = studentPayments
                .OrderByDescending(p => p.PaidOn);

            return View(await studentPayments.ToListAsync());
        }


        //====================================================
        // DETAILS
        //====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Student)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student == null) return NotFound();

                if (payment.Invoice.StudentId != student.StudentId)
                    return Forbid();
            }

            return View(payment);
        }

        //====================================================
        // CREATE
        //====================================================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int StudentId, Payment payment)
        {
            ViewData["StudentId"] = new SelectList(
                _context.Students.OrderBy(s => s.StudentNumber),
                "StudentId",
                "StudentNumber",
                StudentId);

            if (!ModelState.IsValid)
                return View(payment);

            var invoice = await _context.Invoices
                .Include(i => i.Student)
                .Where(i => i.StudentId == StudentId)
                .OrderByDescending(i => i.CreatedOn)
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                ModelState.AddModelError("", "This student has no invoice.");
                return View(payment);
            }

            decimal balance = invoice.AmountDue - invoice.AmountPaid;

            if (balance <= 0)
            {
                ModelState.AddModelError("", "Invoice already paid.");
                return View(payment);
            }

            if (payment.Amount > balance)
            {
                ModelState.AddModelError("Amount",
                    $"Cannot exceed balance ({balance:C})");
                return View(payment);
            }

            payment.InvoiceId = invoice.InvoiceId;
            payment.PaidOn = DateTime.UtcNow;
            payment.RecordedByUserId = User.Identity?.Name ?? "System";

            invoice.AmountPaid += payment.Amount;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _context.Audits.Add(new Audit
            {
                Action = "Payment Created",
                EntityName = "Payment",
                EntityId = payment.PaymentId,
                PerformedByUserId = User.Identity?.Name ?? "System",
                Details = $"Payment {payment.Amount:C} for {invoice.Student.StudentNumber}",
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
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            ViewData["StudentId"] = new SelectList(
                _context.Students,
                "StudentId",
                "StudentNumber",
                payment.Invoice.StudentId);

            return View(payment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int StudentId, Payment payment)
        {
            if (id != payment.PaymentId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(payment);

            var existingPayment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (existingPayment == null)
                return NotFound();

            var oldInvoice = await _context.Invoices.FindAsync(existingPayment.InvoiceId);
            if (oldInvoice != null)
                oldInvoice.AmountPaid -= existingPayment.Amount;

            var newInvoice = await _context.Invoices
                .Include(i => i.Student)
                .Where(i => i.StudentId == StudentId)
                .OrderByDescending(i => i.CreatedOn)
                .FirstOrDefaultAsync();

            if (newInvoice == null)
                return View(payment);

            decimal balance = newInvoice.AmountDue - newInvoice.AmountPaid;

            if (payment.Amount > balance)
                return View(payment);

            payment.InvoiceId = newInvoice.InvoiceId;
            payment.PaidOn = existingPayment.PaidOn;
            payment.RecordedByUserId = User.Identity?.Name ?? "System";

            newInvoice.AmountPaid += payment.Amount;

            _context.Update(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //====================================================
        // DELETE
        //====================================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Student)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Student)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.Invoice.AmountPaid -= payment.Amount;
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //====================================================
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}