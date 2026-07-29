using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SchoolSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public string Description { get; set; } = string.Empty; // e.g. "2026 Tuition - Semester 1"

        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountDue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountPaid { get; set; }

        [NotMapped]
        public decimal Balance => AmountDue - AmountPaid;

        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();








    }
}
