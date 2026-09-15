using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _passwordHasher;

        public UserController(AppDbContext db, IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] UserRoleEnum? role,
            [FromQuery] Guid? agencyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Users.Include(u => u.Agency).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(q) ||
                                         u.FullName.ToLower().Contains(q) ||
                                         u.Email.ToLower().Contains(q));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (agencyId.HasValue)
            {
                query = query.Where(u => u.AgencyId == agencyId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query.OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    AgencyId = u.AgencyId,
                    AgencyCode = u.Agency != null ? u.Agency.Code : null,
                    AgencyName = u.Agency != null ? u.Agency.Name : null,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username và Mật khẩu là bắt buộc." });
            }

            var usernameClean = dto.Username.Trim().ToLower();
            if (await _db.Users.AnyAsync(u => u.Username.ToLower() == usernameClean))
            {
                return BadRequest(new { message = $"Tên đăng nhập '{dto.Username}' đã tồn tại trong hệ thống." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username.Trim() : dto.FullName.Trim(),
                Email = dto.Email?.Trim() ?? string.Empty,
                Role = dto.Role,
                AgencyId = dto.AgencyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var agency = dto.AgencyId.HasValue ? await _db.Agencies.FindAsync(dto.AgencyId.Value) : null;

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AgencyId = user.AgencyId,
                AgencyCode = agency?.Code,
                AgencyName = agency?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _db.Users.Include(u => u.Agency).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            user.FullName = string.IsNullOrWhiteSpace(dto.FullName) ? user.FullName : dto.FullName.Trim();
            user.Email = dto.Email?.Trim() ?? user.Email;
            user.Role = dto.Role;
            user.AgencyId = dto.AgencyId;
            user.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            var agency = dto.AgencyId.HasValue ? await _db.Agencies.FindAsync(dto.AgencyId.Value) : null;

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AgencyId = user.AgencyId,
                AgencyCode = agency?.Code,
                AgencyName = agency?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "Mật khẩu mới không được để trống." });
            }

            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword.Trim());
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Đã đặt lại mật khẩu cho tài khoản '{user.Username}' thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Prevent deleting default admin account
            if (user.Username.ToLower() == "admin")
            {
                return BadRequest(new { message = "Không thể xóa tài khoản Admin hệ thống mặc định." });
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Đã xóa tài khoản '{user.Username}' khỏi hệ thống." });
        }
    }
}
