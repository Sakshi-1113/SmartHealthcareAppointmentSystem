namespace SmartHealthCareAPI.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}