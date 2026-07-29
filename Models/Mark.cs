using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace SchoolSystem.Models
{
    public enum AssessmentType
    {
        Assignment,
        Test,
        Practical,
        Exam
    }
    public class Mark
    {
        public int MarkId { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ModuleId { get; set; }
        public Module? Module { get; set; }

        public AssessmentType AssessmentType { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal Score { get; set; }

        public string RecordedByUserId { get; set; } = string.Empty; // lecturer who captured it
        public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
        public DateTime? LastEditedOn { get; set; }
    }
















}
