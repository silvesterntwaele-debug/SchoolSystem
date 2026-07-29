
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models
{
    public class Module
    {
       public int ModuleId { get; set; }

        [Required, StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }

        public int Capacity { get; set; } = 100;

        public ICollection<StudentModuleMarkSheet> StudentModuleMarkSheets { get; set; }
         = new List<StudentModuleMarkSheet>();

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<Mark> Marks { get; set; } = new List<Mark>();

        public ExamTimetable? ExamTimetable { get; set; }

    
    
    
    
    
    
    
    }
}
