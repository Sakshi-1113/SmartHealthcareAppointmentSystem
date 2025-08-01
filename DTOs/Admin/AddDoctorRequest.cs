namespace SmartHealthCareAPI.DTOs.Admin
{
    public class AddDoctorRequest
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Specialty { get; set; }

        public string Location { get; set; }
    }
}