using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthCareAPI.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        public string Notes { get; set; }

        // Navigation
        public Appointment Appointment { get; set; }
    }
}