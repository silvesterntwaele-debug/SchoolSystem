using System.ComponentModel.DataAnnotations;


namespace SchoolSystem.Models
{
    public class ExamTimetable
    {
        public int  ExamTimetableId { get; set; }

        public int ModuleId { get; set; }
        public Module? Module { get; set; }

        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }

        public string Venue { get; set; } = string.Empty;









    }
}
