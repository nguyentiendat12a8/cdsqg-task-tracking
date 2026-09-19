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
            [FromQuery] string? role,
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

            if (!string.IsNullOrWhiteSpace(role) && role != "all")
            {
                if (Enum.TryParse<UserRoleEnum>(role, true, out var parsedRole))
                {
                    query = query.Where(u => u.Role == parsedRole);
                }
            }

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(u => u.AgencyId == agencyId.Value);
            }

            var leadCounts = await _db.GoalTaskItems
                .Where(g => g.LeadAgencyId != Guid.Empty)
                .Select(g => g.LeadAgencyId)
                .Distinct()
                .ToListAsync();

            var urgeCounts = await _db.TaskUrgeLogs
                .Where(uLog => uLog.LeadAgencyId != null)
                .Select(uLog => uLog.LeadAgencyId!.Value)
                .Distinct()
                .ToListAsync();

            var usedAgencyIds = new HashSet<Guid>(leadCounts.Concat(urgeCounts));
            bool anyDataInSystem = await _db.GoalTaskItems.AnyAsync() || await _db.TaskUrgeLogs.AnyAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var rawItems = await query.OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rawItems.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                AgencyId = u.AgencyId,
                AgencyCode = u.Agency?.Code,
                AgencyName = u.Agency?.Name,
                IsActive = u.IsActive,
                HasDataOperations = (u.Role == UserRoleEnum.Admin && anyDataInSystem) || (u.AgencyId.HasValue && usedAgencyIds.Contains(u.AgencyId.Value)),
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            }).ToList();

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
                return BadRequest(new { message = "Tên đăng nhập và Mật khẩu là bắt buộc." });
            }

            var usernameClean = dto.Username.Trim().ToLower();
            if (await _db.Users.AnyAsync(u => u.Username.ToLower() == usernameClean))
            {
                return BadRequest(new { message = $"Tên đăng nhập '{dto.Username}' đã tồn tại trong hệ thống." });
            }

            var userRole = UserRoleEnum.AgencyUser;
            if (!string.IsNullOrWhiteSpace(dto.RoleString))
            {
                Enum.TryParse(dto.RoleString, true, out userRole);
            }
            else
            {
                userRole = dto.Role;
            }

            Guid? targetAgencyId = (dto.AgencyId.HasValue && dto.AgencyId.Value != Guid.Empty) ? dto.AgencyId : null;
            if (userRole == UserRoleEnum.Admin)
            {
                targetAgencyId = null;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username.Trim() : dto.FullName.Trim(),
                Email = dto.Email?.Trim() ?? string.Empty,
                Role = userRole,
                AgencyId = targetAgencyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var agency = user.AgencyId.HasValue ? await _db.Agencies.FindAsync(user.AgencyId.Value) : null;

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
                HasDataOperations = false,
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

            var userRole = user.Role;
            if (!string.IsNullOrWhiteSpace(dto.RoleString))
            {
                Enum.TryParse(dto.RoleString, true, out userRole);
            }
            else
            {
                userRole = dto.Role;
            }

            Guid? targetAgencyId = (dto.AgencyId.HasValue && dto.AgencyId.Value != Guid.Empty) ? dto.AgencyId : null;
            if (userRole == UserRoleEnum.Admin)
            {
                targetAgencyId = null;
            }

            // Check if user has data operations in the system
            bool anyDataInSystem = await _db.GoalTaskItems.AnyAsync() || await _db.TaskUrgeLogs.AnyAsync();
            bool userAgencyHasData = user.AgencyId.HasValue && (await _db.GoalTaskItems.AnyAsync(g => g.LeadAgencyId == user.AgencyId.Value) || await _db.TaskUrgeLogs.AnyAsync(l => l.LeadAgencyId == user.AgencyId.Value));
            bool hasData = (user.Role == UserRoleEnum.Admin && anyDataInSystem) || userAgencyHasData;

            if (hasData)
            {
                if (userRole != user.Role || targetAgencyId != user.AgencyId)
                {
                    return BadRequest(new { message = "Tài khoản đã phát sinh dữ liệu trong hệ thống, không được phép thay đổi Vai trò và Cơ quan gắn." });
                }
            }

            user.FullName = string.IsNullOrWhiteSpace(dto.FullName) ? user.FullName : dto.FullName.Trim();
            user.Email = dto.Email?.Trim() ?? user.Email;
            user.Role = userRole;
            user.AgencyId = targetAgencyId;
            user.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            var agency = user.AgencyId.HasValue ? await _db.Agencies.FindAsync(user.AgencyId.Value) : null;

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
