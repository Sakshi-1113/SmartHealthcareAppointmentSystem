using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthCareAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string Location { get; set; }

        // Navigation
        public User User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}