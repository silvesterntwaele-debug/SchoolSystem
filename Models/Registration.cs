using System.ComponentModel.DataAnnotations;



namespace SchoolSystem.Models
{
   
        public enum RegistrationStatus
        {
            Registered,
            Dropped,
            Completed
        }

        public class Registration
        {
            public int RegistrationId { get; set; }

            public int StudentId { get; set; }
            public Student? Student { get; set; }

            public int ModuleId { get; set; }
            public Module? Module { get; set; }

            public string Semester { get; set; } = string.Empty; // e.g. "2026-S1"

            public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;

            public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;







        }
}
