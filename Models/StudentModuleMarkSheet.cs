using SchoolSystem.Models;


namespace SchoolSystem.Models;
public class StudentModuleMarkSheet
{
    public int StudentModuleMarkSheetId { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int ModuleId { get; set; }
    public Module? Module { get; set; }

    // Tests
    public decimal? Test1 { get; set; }
    public decimal? Test2 { get; set; }
    public decimal? Test3 { get; set; }

    // Assignments
    public decimal? Assignment1 { get; set; }
    public decimal? Assignment2 { get; set; }

    // Other assessments
    public decimal? Practical { get; set; }
    public decimal? Project { get; set; }

    // Examination
    public decimal? Exam { get; set; }

    // Automatically calculated
    public decimal FinalMark { get; set; }

    public DateTime LastUpdated { get; set; }
}
