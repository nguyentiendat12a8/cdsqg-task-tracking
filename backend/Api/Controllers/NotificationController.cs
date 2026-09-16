using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificationController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] Guid? userId,
            [FromQuery] Guid? agencyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var query = _db.Notifications.AsQueryable();

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                query = query.Where(n => n.UserId == userId.Value || !n.UserId.HasValue);
            }

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(n => n.AgencyId == agencyId.Value || !n.AgencyId.HasValue);
            }

            var totalCount = await query.CountAsync();
            var unreadCount = await query.CountAsync(n => !n.IsRead);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items,
                totalCount,
                unreadCount,
                pageNumber,
                pageSize
            });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notif = await _db.Notifications.FindAsync(id);
            if (notif == null)
            {
                return NotFound(new { message = "Không tìm thấy thông báo." });
            }

            notif.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid? userId, [FromQuery] Guid? agencyId)
        {
            var query = _db.Notifications.Where(n => !n.IsRead);
            if (userId.HasValue) query = query.Where(n => n.UserId == userId.Value);
            if (agencyId.HasValue) query = query.Where(n => n.AgencyId == agencyId.Value);

            var unreadList = await query.ToListAsync();
            foreach (var n in unreadList)
            {
                n.IsRead = true;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã đánh dấu tất cả thông báo là đã đọc." });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { message = "Tiêu đề và nội dung thông báo không được để trống." });
            }

            var targetAgencyIds = new System.Collections.Generic.HashSet<Guid>();

            if (dto.SendToLeadAgency && dto.LeadAgencyId.HasValue && dto.LeadAgencyId.Value != Guid.Empty)
            {
                targetAgencyIds.Add(dto.LeadAgencyId.Value);
            }

            if (dto.CoordinatingAgencyIds != null)
            {
                foreach (var id in dto.CoordinatingAgencyIds)
                {
                    if (id != Guid.Empty) targetAgencyIds.Add(id);
                }
            }

            if (!targetAgencyIds.Any())
            {
                return BadRequest(new { message = "Vui lòng chọn ít nhất một đơn vị nhận thông báo." });
            }

            var createdNotifications = new System.Collections.Generic.List<Notification>();
            foreach (var agencyId in targetAgencyIds)
            {
                var notif = new Notification
                {
                    Id = Guid.NewGuid(),
                    AgencyId = agencyId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = string.IsNullOrWhiteSpace(dto.Type) ? "TaskReminder" : dto.Type,
                    IsRead = false,
                    LinkUrl = dto.TaskId.HasValue ? $"/document-detail?taskId={dto.TaskId}" : null,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Notifications.Add(notif);
                createdNotifications.Add(notif);
            }

            // Record TaskUrgeLog if TaskId is provided for task urge tracking history
            if (dto.TaskId.HasValue && dto.TaskId.Value != Guid.Empty)
            {
                var taskLog = new TaskUrgeLog
                {
                    Id = Guid.NewGuid(),
                    GoalTaskId = dto.TaskId.Value,
                    LeadAgencyId = dto.LeadAgencyId,
                    UrgeContent = dto.Message,
                    CreatedBy = "Đầu mối chỉ đạo CĐS",
                    CreatedAt = DateTime.UtcNow
                };
                _db.TaskUrgeLogs.Add(taskLog);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã gửi thông báo thành công tới {targetAgencyIds.Count} đơn vị!",
                count = targetAgencyIds.Count
            });
        }
    }

    public class SendNotificationRequestDto
    {
        public Guid? TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "TaskReminder";
        public Guid? LeadAgencyId { get; set; }
        public System.Collections.Generic.List<Guid> CoordinatingAgencyIds { get; set; } = new();
        public bool SendToLeadAgency { get; set; } = true;
    }
}
