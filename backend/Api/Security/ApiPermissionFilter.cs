using System.Security.Claims;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Api.Security;

// Runs after authentication. Never use client-supplied identity to grant access.
public sealed class ApiPermissionFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        if (controller == "Auth") { await next(); return; }
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true) { context.Result = new UnauthorizedResult(); return; }
        var admin = user.IsInRole(nameof(UserRoleEnum.Admin));
        Guid? agencyId = Guid.TryParse(user.FindFirstValue("AgencyId"), out var aid) ? aid : null;
        Guid? userId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
        var action = context.RouteData.Values["action"]?.ToString();
        var args = context.ActionArguments;
        if (args.ContainsKey("userRole")) args["userRole"] = admin ? "Admin" : "AgencyUser";
        if (args.ContainsKey("userAgencyId")) args["userAgencyId"] = agencyId;
        if (args.ContainsKey("isAdmin")) args["isAdmin"] = admin;
        if (args.ContainsKey("userId")) args["userId"] = userId;
        if (controller == "Notification" && args.ContainsKey("agencyId")) args["agencyId"] = agencyId;
        if (args.ContainsKey("approvedBy")) args["approvedBy"] = user.FindFirstValue("FullName") ?? user.Identity?.Name;

        bool write = !HttpMethods.IsGet(context.HttpContext.Request.Method)
            && !HttpMethods.IsHead(context.HttpContext.Request.Method);
        // User records and maintenance endpoints are private to administrators.
        if (!admin && (controller is "User" or "MasterData")) { context.Result = new ForbidResult(); return; }
        if (controller == "Notification" && action == "MarkAsRead" && args.TryGetValue("id", out var nid))
        {
            var notification = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.Id == (Guid)nid!);
            if (notification != null && !admin && !(notification.UserId == userId
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
            if (!admin && !agencyId.HasValue) { context.Result = new ForbidResult(); return; }
        }

        if (write && !admin)
        {
            bool allowed = controller == "Execution" && action is "SubmitProgress" or "ImportProgressBulk"
                || controller == "Notification" && action is "MarkAsRead" or "MarkAllAsRead";
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

    private async Task<bool> CanReportAsync(Guid taskId, Guid agencyId)
    {
        return await db.GoalTaskItems.AnyAsync(t => t.Id == taskId &&
            (t.LeadAgencyId == agencyId || t.AssignedAgencyId == agencyId
             || (t.IsGeneralTask && db.Agencies.Any(a => a.Id == agencyId && a.ParentId == null))
             || db.Agencies.Any(a => a.ParentId == agencyId && (a.Id == t.LeadAgencyId || a.Id == t.AssignedAgencyId))));
    }
}
