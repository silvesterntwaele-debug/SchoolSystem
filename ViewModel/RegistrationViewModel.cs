using SchoolSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.ViewModels
{
    public class RegistrationViewModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public string Semester { get; set; }

        public List<int> SelectedModules { get; set; } = new();

        public List<Module> Modules { get; set; } = new();
    }
}