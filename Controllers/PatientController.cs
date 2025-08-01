using Microsoft.AspNetCore.Authorization;
using SmartHealthCareAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthCareAPI.Data;
using SmartHealthCareAPI.Models;
using SmartHealthCareAPI.DTOs;
using System.Security.Claims;

namespace SmartHealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 1. Get all available doctors
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    d.Specialty,
                    d.Location,
                    DoctorName = d.User.Name,
                    DoctorEmail = d.User.Email
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // ✅ 2. Book an appointment
        [HttpPost("appointments")]
        public async Task<IActionResult> BookAppointment(AppointmentRequest request)
        {
            var patientEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var patient = await _context.Users.FirstOrDefaultAsync(u => u.Email == patientEmail);

            if (patient == null || patient.Role != "Patient")
                return Unauthorized("Invalid patient.");

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                PatientId = patient.Id
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok("Appointment booked successfully.");
        }

        // ✅ 3. Get patient appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var patient = await _context.Users.FirstOrDefaultAsync(u => u.Email == patientEmail);

            if (patient == null || patient.Role != "Patient")
                return Unauthorized("Invalid patient.");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.Id)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    DoctorName = a.Doctor.User.Name,
                    DoctorEmail = a.Doctor.User.Email
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ 4. Cancel an appointment
        [HttpPut("appointments/{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var patientEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var patient = await _context.Users.FirstOrDefaultAsync(u => u.Email == patientEmail);

            if (patient == null || patient.Role != "Patient")
                return Unauthorized("Invalid patient.");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            if (appointment.Status == "Cancelled")
                return BadRequest("Appointment is already cancelled.");

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok("Appointment cancelled successfully.");
        }

        // ✅ 5. Get prescriptions
        [HttpGet("prescriptions")]
        public async Task<IActionResult> GetPrescriptions()
        {
            var patientEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var patient = await _context.Users.FirstOrDefaultAsync(u => u.Email == patientEmail);

            if (patient == null || patient.Role != "Patient")
                return Unauthorized("Invalid patient.");

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Where(p => p.Appointment.PatientId == patient.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Notes,
                    p.Appointment.AppointmentDate,
                    DoctorName = p.Appointment.Doctor.User.Name
                })
                .ToListAsync();

            return Ok(prescriptions);
        }
        // Search doctors by specialty
[HttpGet("doctors/search-by-specialty")]
public async Task<IActionResult> SearchDoctorsBySpecialty([FromQuery] string specialty)
{
    if (string.IsNullOrWhiteSpace(specialty))
        return BadRequest("Specialty is required.");

    var doctors = await _context.Doctors
        .Include(d => d.User)
        .Where(d => d.Specialty.ToLower().Contains(specialty.ToLower()))
        .Select(d => new
        {
            d.Id,
            d.Specialty,
            d.Location,
            DoctorName = d.User.Name,
            DoctorEmail = d.User.Email
        })
        .ToListAsync();

    return Ok(doctors);
}

// Search doctors by location
[HttpGet("doctors/search-by-location")]
public async Task<IActionResult> SearchDoctorsByLocation([FromQuery] string location)
{
    if (string.IsNullOrWhiteSpace(location))
        return BadRequest("Location is required.");

    var doctors = await _context.Doctors
        .Include(d => d.User)
        .Where(d => d.Location.ToLower().Contains(location.ToLower()))
        .Select(d => new
        {
            d.Id,
            d.Specialty,
            d.Location,
            DoctorName = d.User.Name,
            DoctorEmail = d.User.Email
        })
        .ToListAsync();

    return Ok(doctors);
}

    }
}