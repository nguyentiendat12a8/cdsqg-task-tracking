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

        private async Task<System.Collections.Generic.List<Guid>> GetScopedAgencyIdsAsync(Guid agencyId)
        {
            var agency = await _db.Agencies.FindAsync(agencyId);
            if (agency == null) return new System.Collections.Generic.List<Guid> { agencyId };

            if (!agency.ParentId.HasValue)
            {
                var childIds = await _db.Agencies.Where(a => a.ParentId == agencyId && a.IsActive).Select(a => a.Id).ToListAsync();
                var list = new System.Collections.Generic.List<Guid> { agencyId };
                list.AddRange(childIds);
                return list;
            }
            return new System.Collections.Generic.List<Guid> { agencyId };
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] Guid? userId,
            [FromQuery] Guid? agencyId,
            [FromQuery] bool isAdmin = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var query = _db.Notifications.AsQueryable();

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    query = query.Where(n => (n.UserId == userId.Value || (n.UserId == null && n.AgencyId.HasValue && n.AgencyId.Value == agencyId.Value)));
                }
                else
                {
                    query = query.Where(n => n.AgencyId.HasValue && n.AgencyId.Value == agencyId.Value);
                }
            }
            else if (userId.HasValue && userId.Value != Guid.Empty)
            {
                query = query.Where(n => n.UserId == userId.Value);
            }
            else
            {
                return Ok(new
                {
                    items = new System.Collections.Generic.List<Notification>(),
                    totalCount = 0,
                    unreadCount = 0,
                    pageNumber,
                    pageSize
                });
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
        [HttpPost("{id}/read")]
        [HttpGet("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead(
            [FromQuery] Guid? userId, 
            [FromQuery] Guid? agencyId, 
            [FromQuery] bool isAdmin = false)
        {
            var query = _db.Notifications.Where(n => !n.IsRead);

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    query = query.Where(n => (n.UserId == userId.Value || (n.UserId == null && n.AgencyId.HasValue && n.AgencyId.Value == agencyId.Value)));
                }
                else
                {
                    query = query.Where(n => n.AgencyId.HasValue && n.AgencyId.Value == agencyId.Value);
                }
            }
            else if (userId.HasValue && userId.Value != Guid.Empty)
            {
                query = query.Where(n => n.UserId == userId.Value);
            }
            else
            {
                return Ok(new { message = "Không có thông báo nào được cập nhật." });
            }

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

            var targetUserIds = dto.TargetUserIds ?? new System.Collections.Generic.List<Guid>();
            var createdNotifications = new System.Collections.Generic.List<Notification>();
            var recipientNames = new System.Collections.Generic.List<string>();

            string? resolvedLinkUrl = !string.IsNullOrWhiteSpace(dto.LinkUrl)
                ? dto.LinkUrl
                : (dto.TaskId.HasValue ? $"/document-detail?taskId={dto.TaskId}{(!string.IsNullOrWhiteSpace(dto.InitialTab) ? $"&tab={dto.InitialTab}" : "")}" : null);

            if (targetUserIds.Any())
            {
                // Send to specific target user IDs or fallback agency IDs
                var users = await _db.Users.Include(u => u.Agency).Where(u => targetUserIds.Contains(u.Id)).ToListAsync();
                foreach (var u in users)
                {
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = u.Id,
                        AgencyId = u.AgencyId,
                        Title = dto.Title,
                        Message = dto.Message,
                        Type = string.IsNullOrWhiteSpace(dto.Type) ? "TaskReminder" : dto.Type,
                        IsRead = false,
                        LinkUrl = resolvedLinkUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.Notifications.Add(notif);
                    createdNotifications.Add(notif);

                    string agencyStr = u.Agency != null ? u.Agency.Name : "Đầu mối";
                    recipientNames.Add($"{u.FullName} ({agencyStr})");
                }

                // Check if any targetUserIds were fallback Agency IDs
                var remainingIds = targetUserIds.Except(users.Select(u => u.Id)).ToList();
                if (remainingIds.Any())
                {
                    var agencies = await _db.Agencies.Where(a => remainingIds.Contains(a.Id)).ToListAsync();
                    foreach (var ag in agencies)
                    {
                        var agNotif = new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = null,
                            AgencyId = ag.Id,
                            Title = dto.Title,
                            Message = dto.Message,
                            Type = string.IsNullOrWhiteSpace(dto.Type) ? "TaskReminder" : dto.Type,
                            IsRead = false,
                            LinkUrl = resolvedLinkUrl,
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.Notifications.Add(agNotif);
                        createdNotifications.Add(agNotif);
                        recipientNames.Add($"Đầu mối {ag.Name}");
                    }
                }
            }
            else
            {
                // Legacy fallback
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

                var agencies = await _db.Agencies.Where(a => targetAgencyIds.Contains(a.Id)).ToListAsync();
                foreach (var agency in agencies)
                {
                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        AgencyId = agency.Id,
                        Title = dto.Title,
                        Message = dto.Message,
                        Type = string.IsNullOrWhiteSpace(dto.Type) ? "TaskReminder" : dto.Type,
                        IsRead = false,
                        LinkUrl = resolvedLinkUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.Notifications.Add(notif);
                    createdNotifications.Add(notif);
                    recipientNames.Add(agency.Name);
                }
            }

            string recipientsSummaryStr = !string.IsNullOrWhiteSpace(dto.RecipientsSummary)
                ? dto.RecipientsSummary
                : string.Join(", ", recipientNames);

            if (string.IsNullOrWhiteSpace(recipientsSummaryStr))
            {
                recipientsSummaryStr = "Các đơn vị được chọn";
            }

            // Record TaskUrgeLog if TaskId is provided for task urge tracking history
            if (dto.TaskId.HasValue && dto.TaskId.Value != Guid.Empty)
            {
                var task = await _db.GoalTaskItems.FirstOrDefaultAsync(t => t.Id == dto.TaskId.Value);

                var taskLog = new TaskUrgeLog
                {
                    Id = Guid.NewGuid(),
                    GoalTaskId = dto.TaskId.Value,
                    LeadAgencyId = dto.LeadAgencyId,
                    Title = dto.Title,
                    TaskCode = task?.Code ?? "",
                    TaskTitle = task?.Title ?? "",
                    UrgeContent = dto.Message,
                    CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy : "Chuyên viên theo dõi CĐS",
                    RecipientsSummary = recipientsSummaryStr,
                    CreatedAt = DateTime.UtcNow
                };
                _db.TaskUrgeLogs.Add(taskLog);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Gửi thông báo thành công.",
                count = createdNotifications.Count,
                recipientsSummary = recipientsSummaryStr
            });
        }

        [HttpGet("recipients-for-task")]
        public async Task<IActionResult> GetRecipientsForTask(
            [FromQuery] Guid? leadAgencyId,
            [FromQuery] string? coordinatingAgencyIds,
            [FromQuery] bool isAdmin = false)
        {
            var recipients = new System.Collections.Generic.List<RecipientOptionDto>();
            var addedAgencyIds = new System.Collections.Generic.HashSet<Guid>();
            var allAgencies = await _db.Agencies.Include(a => a.ParentAgency).Where(a => a.IsActive).ToListAsync();

            void AddAgencyRecipient(Agency agency, string roleTag, string infoSummary)
            {
                if (agency == null || addedAgencyIds.Contains(agency.Id)) return;
                addedAgencyIds.Add(agency.Id);
                recipients.Add(new RecipientOptionDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    AgencyId = agency.Id,
                    AgencyName = agency.Name,
                    AgencyCode = agency.Code,
                    RoleTag = roleTag,
                    DisplayLabel = $"{agency.Name} ({roleTag})",
                    InfoSummary = infoSummary
                });
            }

            void AddAgencyAndHierarchy(Guid agId, string mainRoleTag, string defaultInfoSummary)
            {
                var agency = allAgencies.FirstOrDefault(a => a.Id == agId);
                if (agency == null) return;

                // 1. Add the target agency itself
                string agencySummary = defaultInfoSummary;
                if (agency.ParentAgency != null && !string.IsNullOrWhiteSpace(agency.ParentAgency.Name))
                {
                    agencySummary = $"Trực thuộc {agency.ParentAgency.Name}";
                }
                AddAgencyRecipient(agency, mainRoleTag, agencySummary);

                // 2. If this agency has a Parent Agency (e.g. Cục thuộc Bộ):
                // Add the Parent Agency and all sibling units belonging to the Parent Ministry!
                if (agency.ParentId.HasValue && agency.ParentId.Value != Guid.Empty)
                {
                    var parentAgency = allAgencies.FirstOrDefault(a => a.Id == agency.ParentId.Value);
                    if (parentAgency != null)
                    {
                        AddAgencyRecipient(parentAgency, "Cơ quan cấp trên", "Bộ/Cơ quan quản lý trực tiếp");

                        // Add sibling sub-agencies under the same Parent Ministry
                        var siblings = allAgencies.Where(a => a.ParentId == parentAgency.Id && a.Id != agency.Id).ToList();
                        foreach (var sib in siblings)
                        {
                            AddAgencyRecipient(sib, "Đơn vị thuộc Bộ", $"Đơn vị trực thuộc {parentAgency.Name}");
                        }
                    }
                }

                // 3. Add child sub-agencies directly under this agency (if any)
                var childSubAgencies = allAgencies.Where(a => a.ParentId == agency.Id).ToList();
                foreach (var sub in childSubAgencies)
                {
                    AddAgencyRecipient(sub, "Đơn vị trực thuộc", $"Đơn vị trực thuộc {agency.Name}");
                }
            }

            // 1. Lead Agency & its Ministry / Sibling / Child units
            if (leadAgencyId.HasValue && leadAgencyId.Value != Guid.Empty)
            {
                AddAgencyAndHierarchy(leadAgencyId.Value, "Đơn vị chủ trì", "Cơ quan chủ trì chính");
            }

            // 2. Coordinating Agencies & their Ministry / Sibling / Child units
            if (!string.IsNullOrWhiteSpace(coordinatingAgencyIds))
            {
                var parts = coordinatingAgencyIds.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (Guid.TryParse(p.Trim(), out var cId))
                    {
                        AddAgencyAndHierarchy(cId, "Đơn vị phối hợp", "Cơ quan phối hợp");
                    }
                }
            }

            return Ok(recipients);
        }
    }

    public class SendNotificationRequestDto
    {
        public Guid? TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "TaskReminder";
        public Guid? LeadAgencyId { get; set; }
        public System.Collections.Generic.List<Guid>? TargetUserIds { get; set; } = new();
        public System.Collections.Generic.List<Guid>? CoordinatingAgencyIds { get; set; } = new();
        public bool SendToLeadAgency { get; set; } = true;
        public string? CreatedBy { get; set; }
        public string? RecipientsSummary { get; set; }
        public string? LinkUrl { get; set; }
        public string? InitialTab { get; set; }
    }

    public class RecipientOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string AgencyCode { get; set; } = string.Empty;
        public string RoleTag { get; set; } = string.Empty;
        public string DisplayLabel { get; set; } = string.Empty;
        public string InfoSummary { get; set; } = string.Empty;
    }
}
