using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
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
        private readonly PasswordRecoveryService? _recovery;

        public AuthController(AppDbContext db, IPasswordHasher passwordHasher, IJwtService jwtService, PasswordRecoveryService? recovery = null)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _recovery = recovery;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        [EnableRateLimiting("password-recovery")]
        [HttpPost("forgot-password")]
        public Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            return Task.FromResult<IActionResult>(Ok(new { message = "Tính năng đang phát triển" }));
        }
        [AllowAnonymous]
        [EnableRateLimiting("password-recovery")]
        [HttpPost("reset-password")]
        public Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordDto dto)
        {
            return Task.FromResult<IActionResult>(StatusCode(501, new { message = "Tính năng đang phát triển" }));
        }
    }
}
