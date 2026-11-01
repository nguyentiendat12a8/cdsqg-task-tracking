using System.Data;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.Services;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Cdsqg.Application.AI;
public sealed class AiReviewService(AppDbContext db, IExecutionService execution, AiInboxService? inbox = null, IWebHostEnvironment? env = null)
{
    public async Task ReviewAsync(Guid id,bool approve,decimal? acceptedValue,string? note,string actor,CancellationToken ct)
    {
        note=(note??"").Trim();
        if(note.Length>2000)throw new InvalidOperationException("Ghi chú tối đa 2.000 ký tự.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var item=await db.AiDataSuggestions.SingleOrDefaultAsync(s=>s.Id==id,ct) ?? throw new KeyNotFoundException();
        if(item.Status!="Pending")throw new InvalidOperationException("Đề xuất đã được xử lý; hãy tải lại danh sách.");
        if(approve) {
            var metric=await AiMetricCatalog.ResolveAsync(db,item.MetricKey,ct);var value=acceptedValue??item.Value;
            if(value<0 || ((metric.Unit=="rank"&&value>193)||(metric.Unit=="%"&&value>100)) || metric.Unit=="rank" && (value<1 || value!=decimal.Truncate(value)))throw new InvalidOperationException("Giá trị kiểm chứng không hợp lệ với đơn vị chỉ số.");
            ReportingPeriod.ValidateYear(item.DataYear);
            var links=await db.AiGoalDataLinks.Where(l=>l.SuggestionId==item.Id).ToListAsync(ct);
            if(links.Count==0)throw new InvalidOperationException("Đề xuất chưa liên kết ID mục tiêu; chưa thể ghi báo cáo.");
            var document=await db.AiSourceDocuments.SingleAsync(d=>d.Id==item.DocumentId,ct);
            var inboxItem = document.InboxItemId.HasValue ? await db.AiInboxItems.FindAsync([document.InboxItemId.Value], ct) : null;

            string inputCode;
            if (inboxItem != null)
            {
                var isUpload = !string.IsNullOrEmpty(inboxItem.StorageKey);
                var vnTime = inboxItem.ReceivedAt.AddHours(7);
                inputCode = $"{(isUpload ? "TL" : "RSS")}-{vnTime:ddMMyyyy}-{inboxItem.SequenceNumber:D3}";
            }
            else
            {
                inputCode = document.Title;
            }

            var confirmTime = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
            var summaryNote = string.IsNullOrWhiteSpace(note)
                ? $"Admin xác nhận tại [{inputCode}] thời gian {confirmTime}."
                : $"Admin xác nhận tại [{inputCode}] thời gian {confirmTime}. Ghi chú: {note.Trim()}";

            string fileUrl = document.Url;
            if (inboxItem != null && !string.IsNullOrEmpty(inboxItem.StorageKey) && inbox != null)
            {
                try
                {
                    var privatePath = inbox.PrivatePath(inboxItem.StorageKey);
                    if (File.Exists(privatePath))
                    {
                        var webRoot = env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var evidenceDir = Path.Combine(webRoot, "uploads", "evidence");
                        if (!Directory.Exists(evidenceDir)) Directory.CreateDirectory(evidenceDir);

                        var cleanTitle = Path.GetFileNameWithoutExtension(inboxItem.Title).Replace(" ", "_").Replace("#", "").Replace("?", "");
                        if (cleanTitle.Length > 40) cleanTitle = cleanTitle[..40];
                        var ext = Path.GetExtension(inboxItem.Title);
                        var uniqueName = $"{Guid.NewGuid()}_{cleanTitle}{ext}";
                        var destPath = Path.Combine(evidenceDir, uniqueName);
                        File.Copy(privatePath, destPath, overwrite: true);
                        fileUrl = $"/uploads/evidence/{uniqueName}";
                    }
                }
                catch
                {
                    fileUrl = document.Url;
                }
            }

            foreach(var link in links)
            {
                var goal=await db.GoalTaskItems.Include(g=>g.Unit).SingleAsync(g=>g.Id==link.GoalTaskId,ct);
                if(goal.ItemType!=ItemTypeEnum.Goal || goal.EvaluationType!=EvaluationTypeEnum.Quantitative || goal.IsGeneralTask && !item.TargetAgencyId.HasValue)
                    throw new InvalidOperationException($"{goal.Code}: cấu hình mục tiêu không phù hợp báo cáo số liệu toàn quốc.");
                if(item.TargetAgencyId.HasValue) {
                    if(!await db.AgencyTaskExecutions.AnyAsync(e=>e.GoalTaskId==goal.Id&&e.AgencyId==item.TargetAgencyId&&e.IsLeadAssignment,ct))throw new InvalidOperationException("Đơn vị không còn được phân giao chủ trì mục tiêu; cần kiểm tra lại.");
                    if((goal.StartDate.HasValue&&goal.StartDate.Value.Year>item.DataYear)||(goal.DueDate.HasValue&&goal.DueDate.Value.Year<item.DataYear))throw new InvalidOperationException("Năm số liệu ngoài thời gian thực hiện mục tiêu.");
                }
                var rank=ProgressCalculator.IsLowerBetter(goal);
                var percent=!goal.UnitId.HasValue || goal.Unit?.Name=="%" || goal.Unit?.Code=="PERCENT";
                if(metric.Unit=="rank"?!rank:metric.Unit=="%"?!percent:percent)
                    throw new InvalidOperationException($"{goal.Code}: đơn vị mục tiêu không khớp số liệu AI; cần kiểm tra cấu hình trước khi duyệt.");
                var result=await execution.SubmitProgressAsync(goal.Id,new SubmitProgressRequestDto {
                    PeriodYear=item.DataYear,Value=value,UserRole="Admin",CreatedBy=actor,
                    AgencyId=item.TargetAgencyId??goal.AssignedAgencyId??goal.LeadAgencyId,
                    SummaryNotes=summaryNote,
                    ExistingFiles=new(){fileUrl}
                });
                link.ProgressLogId=result.ProgressLogId;
                var report=await db.ProgressLogs.SingleAsync(p=>p.Id==result.ProgressLogId,ct);
                report.ApprovedBy=actor;report.ApprovedAt=DateTime.UtcNow;
            }
            // Keep prior decisions as audit history; only one current accepted measurement per metric/year.
            var previous=await db.AiDataSuggestions.Where(s=>s.MetricKey==item.MetricKey && s.DataYear==item.DataYear && s.TargetAgencyId==item.TargetAgencyId && s.IsCurrent).ToListAsync(ct);
            foreach(var old in previous)old.IsCurrent=false;
            await db.SaveChangesAsync(ct);
            item.AcceptedValue=value;item.IsCurrent=true;item.Status="Approved";
        } else {item.Status="Rejected";item.IsCurrent=false;}
        item.ReviewedAt=DateTime.UtcNow;item.ReviewedBy=actor;item.ReviewNote=note.Trim();
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
}
