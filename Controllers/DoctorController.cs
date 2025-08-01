using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthCareAPI.Data;
using SmartHealthCareAPI.Models;
using System.Security.Claims;

namespace SmartHealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ View doctor's own appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments()
        {
            var doctorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var doctor = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Email == doctorEmail);

            if (doctor?.DoctorProfile == null)
                return Unauthorized("Doctor profile not found.");

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorProfile.Id)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    PatientName = a.Patient.Name,
                    PatientEmail = a.Patient.Email
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ Update appointment status (Approve/Reject)
        [HttpPut("appointments/{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] string status)
        {
            var doctorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var doctor = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Email == doctorEmail);

            if (doctor?.DoctorProfile == null)
                return Unauthorized("Doctor profile not found.");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.DoctorProfile.Id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            if (status != "Approved" && status != "Rejected")
                return BadRequest("Invalid status. Use 'Approved' or 'Rejected'.");

            appointment.Status = status;
            await _context.SaveChangesAsync();

            return Ok("Appointment status updated.");
        }

        // ✅ Add prescription to an appointment
        [HttpPost("appointments/{id}/prescription")]
        public async Task<IActionResult> AddPrescription(int id, [FromBody] string notes)
        {
            var doctorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var doctor = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Email == doctorEmail);

            if (doctor?.DoctorProfile == null)
                return Unauthorized("Doctor profile not found.");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.DoctorProfile.Id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            if (appointment.Prescription != null)
                return BadRequest("Prescription already exists for this appointment.");

            var prescription = new Prescription
            {
                AppointmentId = appointment.Id,
                Notes = notes
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return Ok("Prescription added successfully.");
        }
        
    }
}