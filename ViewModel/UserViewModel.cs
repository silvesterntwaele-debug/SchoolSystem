using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? StudentNumber { get; set; }

        public string? StaffNumber { get; set; }
    }
}
