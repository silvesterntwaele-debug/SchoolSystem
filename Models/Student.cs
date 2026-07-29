using System.ComponentModel.DataAnnotations;


namespace SchoolSystem.Models
{
    public class Student
    {
      public int StudentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(9)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Program {  get; set; } = string.Empty;

        public int YearOfStudy { get; set; }

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<Mark> Marks { get; set; } = new List<Mark>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public ICollection<StudentModuleMarkSheet> StudentModuleMarkSheets { get; set; }
              = new List<StudentModuleMarkSheet>();






    }
}
