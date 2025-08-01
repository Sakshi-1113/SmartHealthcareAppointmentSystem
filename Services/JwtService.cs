using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartHealthCareAPI.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;

        public JwtService(IConfiguration config)
        {
            _secretKey = config["Jwt:Key"];     // ✅ fixed casing
            _issuer = config["Jwt:Issuer"];     // ✅ fixed casing
        }

        public string GenerateToken(string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _issuer, // ✅ required!
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}