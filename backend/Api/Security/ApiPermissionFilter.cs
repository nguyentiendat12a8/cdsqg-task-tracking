using System.Security.Claims;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Api.Security;

// Runs after authentication. Never use client-supplied identity to grant access.
public sealed class ApiPermissionFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null
            || context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            await next();
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();
        if (controller is "Auth" or "AiAlertsBridge") { await next(); return; }
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true) { context.Result = new UnauthorizedResult(); return; }
        var admin = user.IsInRole(nameof(UserRoleEnum.Admin));
        var clerk = !admin && user.IsInRole(nameof(UserRoleEnum.Clerk));
        // Admin và Văn thư cùng phạm vi xem toàn hệ thống; quyền ghi tách riêng ở cuối filter (RolePermissions).
        var seesAll = RolePermissions.SeesAllAgencies(admin, clerk);
        Guid? agencyId = Guid.TryParse(user.FindFirstValue("AgencyId"), out var aid) ? aid : null;
        Guid? userId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
        var action = context.RouteData.Values["action"]?.ToString();
        var args = context.ActionArguments;
        if (controller == "Execution" && action is "SubmitProgress" or "GetProgress" or "ImportProgressBulk")
        {
            var request = context.HttpContext.Request;
            var fields = request.HasFormContentType ? (await request.ReadFormAsync()).Keys : request.Query.Keys;
            if (fields.Any(k => new[] { "PeriodQuarter", "PeriodMonth", "PeriodType", "quarter", "period" }.Contains(k, StringComparer.OrdinalIgnoreCase)))
            {
                context.Result = new BadRequestObjectResult(new { message = "Hệ thống chỉ hỗ trợ báo cáo theo năm (PeriodYear)." });
                return;
            }
        }
        if (args.ContainsKey("userRole")) args["userRole"] = seesAll ? "Admin" : "AgencyUser";
        if (args.ContainsKey("userAgencyId")) args["userAgencyId"] = seesAll ? null : agencyId;
        if (args.ContainsKey("isAdmin")) args["isAdmin"] = seesAll;
        if (args.ContainsKey("userId")) args["userId"] = userId;
        if (controller == "Notification" && args.ContainsKey("agencyId")) args["agencyId"] = agencyId;
        // These list endpoints must always use the authenticated agency, including exports.
        if (!seesAll && ((controller == "Document" && action == "GetDocumentItems")
            || (controller == "Planning" && action == "GetDocumentPlanningGrid")))
        {
            if (!agencyId.HasValue || agencyId == Guid.Empty) { context.Result = new ForbidResult(); return; }
            args["agencyId"] = agencyId;
        }
        // Plan import trusts only the authenticated identity, even when the query omits these fields.
        if (controller == "Planning" && action == "ImportYearlyTargets")
        {
            args["userRole"] = admin ? "Admin" : "AgencyUser";
            args["userAgencyId"] = agencyId;
        }
        if (args.ContainsKey("approvedBy")) args["approvedBy"] = user.FindFirstValue("FullName") ?? user.Identity?.Name;

        bool write = !HttpMethods.IsGet(context.HttpContext.Request.Method)
            && !HttpMethods.IsHead(context.HttpContext.Request.Method);
        // User records and maintenance endpoints are private to administrators.
        if (!admin && RolePermissions.IsAdminOnly(controller)) { context.Result = new ForbidResult(); return; }
        if (controller == "Notification" && action == "MarkAsRead" && args.TryGetValue("id", out var nid))
        {
            var notification = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.Id == (Guid)nid!);
            if (notification != null && !seesAll && !(notification.UserId == userId
                || (notification.UserId == null && agencyId.HasValue && notification.AgencyId == agencyId)))
            { context.Result = new ForbidResult(); return; }
        }

        if (controller == "Execution" && action == "SubmitProgress" && args["dto"] is SubmitProgressRequestDto progress)
        {
            progress.UserRole = admin ? "Admin" : "AgencyUser";
            progress.CreatedBy = user.FindFirstValue("FullName") ?? user.Identity?.Name;
            if (!admin) progress.AgencyId = agencyId;
            if (!admin && (!agencyId.HasValue || !await CanReportAsync((Guid)args["taskId"]!, agencyId.Value)))
            { context.Result = new ForbidResult(); return; }
        }
        if (controller == "Execution" && action == "ImportProgressBulk" && args["dto"] is ImportProgressBulkRequestDto bulk)
        {
            bulk.UserRole = admin ? "Admin" : "AgencyUser";
            bulk.UserAgencyId = agencyId;
            bulk.CreatedBy = user.FindFirstValue("FullName") ?? user.Identity?.Name ?? "Người dùng đã xác thực";
            if (!admin && !agencyId.HasValue) { context.Result = new ForbidResult(); return; }
        }

        if (write && clerk)
        {
            // Văn thư: chỉ thao tác dữ liệu phần Tiện ích; các phân hệ khác chỉ xem.
            if (!RolePermissions.ClerkCanWrite(controller, action)) { context.Result = new ForbidResult(); return; }
        }
        else if (write && !admin)
        {
            bool allowed = controller == "Execution" && action is "SubmitProgress" or "ImportProgressBulk"
                || controller == "Notification" && action is "MarkAsRead" or "MarkAllAsRead"
                // Lead-agency ownership of each goal is enforced in PlanningService.ImportYearlyTargetsAsync.
                || controller == "Planning" && action == "ImportYearlyTargets" && agencyId.HasValue;
            if (controller == "Agency" && action is "UpdatePlansAndContacts" or "UploadPlanFile" or "UploadPlanFileLegacy" or "DeletePlanFile")
            {
                var target = args.TryGetValue("id", out var id) ? id : args.TryGetValue("agencyId", out var targetAgency) ? targetAgency : null;
                allowed = agencyId.HasValue && target is Guid targetId && await db.Agencies.AnyAsync(a => a.Id == targetId
                    && (a.Id == agencyId.Value || a.ParentId == agencyId.Value));
            }
            if (controller == "Agency" && action == "DeletePlanFileDirect" && agencyId.HasValue && args["fileId"] is Guid fileId)
            {
                var agencies = await db.Agencies.AsNoTracking()
                    .Where(a => a.Id == agencyId.Value || a.ParentId == agencyId.Value).ToListAsync();
                allowed = agencies.Any(a => a.PlanFiles.Any(f => f.Id == fileId));
            }
            if (!allowed) { context.Result = new ForbidResult(); return; }
        }
        await next();
    }

    private Task<bool> CanReportAsync(Guid taskId, Guid agencyId)
        => Cdsqg.Application.Services.AgencyScopePolicy.CanReportAsync(db, taskId, agencyId);
}
