using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }

        [Required] 
        public string UserID { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(20)]
        public string StaffNumber { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public ICollection<Module> Modules { get; set; } = new List<Module>();
    
    
    
    }
}
