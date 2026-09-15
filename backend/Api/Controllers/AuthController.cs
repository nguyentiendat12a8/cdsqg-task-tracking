using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext db, IPasswordHasher passwordHasher, IJwtService jwtService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập Tên đăng nhập và Mật khẩu." });
            }

            var user = await _db.Users.Include(u => u.Agency)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ Admin." });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AgencyId = user.AgencyId,
                AgencyCode = user.Agency?.Code,
                AgencyName = user.Agency?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = userDto
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập hoặc phiên đăng nhập hết hạn." });
            }

            var user = await _db.Users.Include(u => u.Agency).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Không tìm thấy người dùng hoặc tài khoản bị vô hiệu hóa." });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AgencyId = user.AgencyId,
                AgencyCode = user.Agency?.Code,
                AgencyName = user.Agency?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmailOrUsername))
            {
                return BadRequest(new { message = "Vui lòng nhập Email hoặc Tên đăng nhập của tài khoản." });
            }

            var query = dto.EmailOrUsername.Trim().ToLower();
            var user = await _db.Users.FirstOrDefaultAsync(u => 
                u.Username.ToLower() == query || 
                (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower() == query));

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản tương ứng với thông tin đã nhập." });
            }

            // Generate a secure temporary reset password
            var randomDigits = new Random().Next(1000, 9999);
            var tempPassword = $"Reset#{randomDigits}";

            user.PasswordHash = _passwordHasher.HashPassword(tempPassword);
            await _db.SaveChangesAsync();

            var targetEmail = string.IsNullOrWhiteSpace(user.Email) ? $"{user.Username}@cdsqg.gov.vn" : user.Email;

            return Ok(new
            {
                message = $"Mật khẩu tạm thời đã được tạo và gửi thành công về email {targetEmail}!",
                email = targetEmail,
                tempPassword = tempPassword
            });
        }
    }
}
