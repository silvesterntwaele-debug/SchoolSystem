using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace SchoolSystem.Models
{
    public class Audit
    {

        // Simple audit trail. Written to whenever a mark or fee record changes.
       
            public int AuditId { get; set; }
            public string Action { get; set; } = string.Empty;   // e.g. "MarkEdited", "InvoiceCreated"
            public string EntityName { get; set; } = string.Empty; // e.g. "Mark", "Invoice"
            public int EntityId { get; set; }
            public string PerformedByUserId { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    







    }
}
