using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthCareAPI.Data;
using SmartHealthCareAPI.DTOs.Auth;
using SmartHealthCareAPI.Models;
using SmartHealthCareAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace SmartHealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registration successful.");
        }

       [HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null)
        return Unauthorized("Invalid credentials.");

    var hashedInputPassword = HashPassword(request.Password);

    // Debug logs — remove these in production
    Console.WriteLine("Stored hash: " + user.PasswordHash);
    Console.WriteLine("Input hash: " + hashedInputPassword);

    if (user.PasswordHash != hashedInputPassword)
        return Unauthorized("Invalid credentials.");

    var token = _jwtService.GenerateToken(user.Email, user.Role);

    return Ok(new AuthResponse
    {
        Token = token,
        Email = user.Email,
        Role = user.Role
    });
}

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}