using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models
{
    public class Payment
    {
       
            public int PaymentId { get; set; }

            public int InvoiceId { get; set; }
            public Invoice? Invoice { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            public decimal Amount { get; set; }

            public DateTime PaidOn { get; set; } = DateTime.UtcNow;
            public string? Reference { get; set; }
            public string RecordedByUserId { get; set; } = string.Empty; // admin who captured it











        }
}
