using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthCareAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; } // Renamed from Date

        [Required]
        public string Status { get; set; } // Pending / Approved / Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Added for tracking

        // Navigation properties
        public User Patient { get; set; }
        public Doctor Doctor { get; set; }

        public Prescription Prescription { get; set; }
    }
}