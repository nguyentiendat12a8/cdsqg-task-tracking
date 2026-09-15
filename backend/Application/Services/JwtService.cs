using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Cdsqg.Core.Entities;

namespace Cdsqg.Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private static readonly string FallbackSecret = "CdsqgNationalDigitalTransformationSecretKey2026MustBeAtLeast32BytesLong!";

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var secretKey = _config["Jwt:SecretKey"] ?? FallbackSecret;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("AgencyId", user.AgencyId?.ToString() ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "CdsqgSystem",
                audience: _config["Jwt:Audience"] ?? "CdsqgClients",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
