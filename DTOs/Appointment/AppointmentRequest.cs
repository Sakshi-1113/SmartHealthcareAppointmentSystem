namespace SmartHealthCareAPI.DTOs.Patient
{
    public class AppointmentRequest
    {
        public int DoctorId { get; set; }

        public DateTime AppointmentDate { get; set; }
    }
}