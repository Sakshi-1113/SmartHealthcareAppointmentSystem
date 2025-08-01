using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthCareAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Role { get; set; }  // Admin / Doctor / Patient

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Navigation property (for Doctor only)
        public Doctor DoctorProfile { get; set; }
    }
}