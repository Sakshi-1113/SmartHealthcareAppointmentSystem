using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthCareAPI.Data;
using SmartHealthCareAPI.DTOs.Admin;
using SmartHealthCareAPI.Models;

namespace SmartHealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 1. Get all doctors
        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            return Ok(doctors);
        }

        // ✅ 2. Add doctor
        [HttpPost("doctors")]
        public async Task<IActionResult> AddDoctor([FromBody] AddDoctorRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = "Doctor",
                PasswordHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password))
                )
            };

            var doctor = new Doctor
            {
                User = user,
                Specialty = request.Specialty,
                Location = request.Location
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok("Doctor added successfully.");
        }

        // ✅ 3. Delete doctor
        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound("Doctor not found.");

            // Remove both doctor and user
            _context.Users.Remove(doctor.User);
            await _context.SaveChangesAsync();

            return Ok("Doctor deleted successfully.");
        }

        // ✅ 4. Edit doctor
        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> EditDoctor(int id, [FromBody] EditDoctorRequest request)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound("Doctor not found.");

            // Update user info
            doctor.User.Name = request.Name ?? doctor.User.Name;
            doctor.User.Email = request.Email ?? doctor.User.Email;

            // Update doctor info
            doctor.Specialty = request.Specialty ?? doctor.Specialty;
            doctor.Location = request.Location ?? doctor.Location;

            await _context.SaveChangesAsync();
            return Ok("Doctor updated successfully.");
        }

        // ✅ 5. Get all appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ 6. Get appointment count per doctor
        [HttpGet("appointments/count-by-doctor")]
        public async Task<IActionResult> GetAppointmentCountPerDoctor()
        {
            var doctorAppointmentCounts = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .GroupBy(a => new { a.DoctorId, a.Doctor.User.Name, a.Doctor.User.Email })
                .Select(group => new
                {
                    DoctorId = group.Key.DoctorId,
                    DoctorName = group.Key.Name,
                    DoctorEmail = group.Key.Email,
                    AppointmentCount = group.Count()
                })
                .ToListAsync();

            return Ok(doctorAppointmentCounts);
        }
        // ✅ Get appointment count per patient
[HttpGet("appointments/count-by-patient")]
public async Task<IActionResult> GetAppointmentCountPerPatient()
{
    var patientAppointmentCounts = await _context.Appointments
        .Include(a => a.Patient)
        .GroupBy(a => new { a.PatientId, a.Patient.Name, a.Patient.Email })
        .Select(group => new
        {
            PatientId = group.Key.PatientId,
            PatientName = group.Key.Name,
            PatientEmail = group.Key.Email,
            AppointmentCount = group.Count()
        })
        .ToListAsync();

    return Ok(patientAppointmentCounts);
}
    }

}