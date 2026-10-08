using System.Security.Claims;
using Cdsqg.Api.Controllers;
using Cdsqg.Api.Security;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cdsqg.Tests;

public class SecurityTests
{
    private static AppDbContext Database() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ActionExecutingContext Context(string controller, string action, string method,
        Dictionary<string, object?>? args = null, Guid? agencyId = null, bool admin = false, bool authenticated = true)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = method;
        if (authenticated)
            http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, admin ? "Admin" : "AgencyUser"),
                new Claim("AgencyId", agencyId?.ToString() ?? ""),
                new Claim("FullName", "Verified user") }, "test"));
        var routes = new RouteData();
        routes.Values["controller"] = controller;
        routes.Values["action"] = action;
        return new ActionExecutingContext(new ActionContext(http, routes, new ActionDescriptor()),
            new List<IFilterMetadata>(), args ?? new(), new object());
    }

    private static async Task<bool> Run(AppDbContext db, ActionExecutingContext ctx)
    {
        bool called = false;
        await new ApiPermissionFilter(db).OnActionExecutionAsync(ctx, () => {
            called = true;
            return Task.FromResult(new ActionExecutedContext(ctx, new List<IFilterMetadata>(), ctx.Controller));
        });
        return called;
    }

    [Theory]
    [InlineData("PeriodQuarter")]
    [InlineData("PeriodMonth")]
    [InlineData("PeriodType")]
    public async Task RetiredReportFieldsAreRejectedRatherThanSavedAsAnnual(string field)
    {
        using var db = Database();
        var ctx = Context("Execution", "SubmitProgress", "POST", admin: true);
        ctx.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        ctx.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { [field] = "1" });
        Assert.False(await Run(db, ctx));
        Assert.IsType<BadRequestObjectResult>(ctx.Result);
    }

    [Fact]
    public void ChildCreationAndQuarterlyImportAreRejectedByTheJsonContract()
    {
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Assert.Throws<System.Text.Json.JsonException>(() => System.Text.Json.JsonSerializer.Deserialize<CreateGoalTaskItemRequestDto>("{\"parentId\":\"11111111-1111-1111-1111-111111111111\"}", options));
        Assert.Throws<System.Text.Json.JsonException>(() => System.Text.Json.JsonSerializer.Deserialize<ImportProgressBulkItemDto>("{\"periodQuarter\":1}", options));
    }

    [Fact]
    public async Task AnonymousCannotAccessMaintenance()
    {
        using var db = Database();
        var ctx = Context("MasterData", "ClearDemoData", "POST", authenticated: false);
        Assert.False(await Run(db, ctx));
        Assert.IsType<UnauthorizedResult>(ctx.Result);
    }

    [Theory]
    [InlineData("User", "CreateUser", "POST")]
    [InlineData("User", "GetUsers", "GET")]
    [InlineData("MasterData", "ResetProgressData", "DELETE")]
    [InlineData("Execution", "ApproveProgress", "POST")]
    [InlineData("LegalDocument", "ClearAllLegalDocuments", "DELETE")]
    public async Task AgencyCannotUseAdminActions(string controller, string action, string method)
    {
        using var db = Database();
        var ctx = Context(controller, action, method);
        Assert.False(await Run(db, ctx));
        Assert.IsType<ForbidResult>(ctx.Result);
    }

    [Fact]
    public async Task AdminCanUseMaintenance()
    {
        using var db = Database();
        Assert.True(await Run(db, Context("MasterData", "ResetProgressData", "POST", admin: true)));
    }

    [Fact]
    public async Task ProgressIdentityCannotBeSpoofed()
    {
        using var db = Database();
        var agencyId = Guid.NewGuid();
        var task = new GoalTaskItem { LeadAgencyId = agencyId };
        db.GoalTaskItems.Add(task);
        await db.SaveChangesAsync();
        var dto = new SubmitProgressRequestDto { UserRole = "Admin", AgencyId = Guid.NewGuid(), CreatedBy = "admin" };
        var ctx = Context("Execution", "SubmitProgress", "POST",
            new() { ["taskId"] = task.Id, ["dto"] = dto }, agencyId);
        Assert.True(await Run(db, ctx));
        Assert.Equal("AgencyUser", dto.UserRole);
        Assert.Equal(agencyId, dto.AgencyId);
        Assert.Equal("Verified user", dto.CreatedBy);
    }

    [Fact]
    public async Task AgencyCannotReportAnotherAgencysTask()
    {
        using var db = Database();
        var task = new GoalTaskItem { LeadAgencyId = Guid.NewGuid() };
        db.GoalTaskItems.Add(task);
        await db.SaveChangesAsync();
        var ctx = Context("Execution", "SubmitProgress", "POST",
            new() { ["taskId"] = task.Id, ["dto"] = new SubmitProgressRequestDto { UserRole = "Admin" } }, Guid.NewGuid());
        Assert.False(await Run(db, ctx));
        Assert.IsType<ForbidResult>(ctx.Result);
    }

    [Fact]
    public async Task BulkImportIdentityCannotBeSpoofed()
    {
        using var db = Database();
        var agencyId = Guid.NewGuid();
        var dto = new ImportProgressBulkRequestDto { UserRole = "Admin", UserAgencyId = Guid.NewGuid() };
        Assert.True(await Run(db, Context("Execution", "ImportProgressBulk", "POST", new() { ["dto"] = dto }, agencyId)));
        Assert.Equal("AgencyUser", dto.UserRole);
        Assert.Equal(agencyId, dto.UserAgencyId);
    }

    [Theory]
    [InlineData("known")]
    [InlineData("unknown")]
    public async Task ForgotPasswordDoesNotChangeOrDiscloseCredentials(string name)
    {
        using var db = Database();
        var user = new User { Username = "known", PasswordHash = "unchanged" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var controller = new AuthController(db, new PasswordHasher(), new JwtService(new ConfigurationBuilder().Build()));
        var result = Assert.IsType<OkObjectResult>(await controller.ForgotPassword(new ForgotPasswordDto { EmailOrUsername = name }));
        var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain("tempPassword", json);
        using var payload = System.Text.Json.JsonDocument.Parse(json);
        Assert.Single(payload.RootElement.EnumerateObject());
        Assert.Equal("unchanged", (await db.Users.FindAsync(user.Id))!.PasswordHash);
    }

    [Fact]
    public void DatabaseUrlAloneSelectsPostgreSqlAndOverridesLocalConnection()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["DATABASE_URL"] = "postgres://deployment/db", ["ConnectionStrings:DefaultConnection"] = "local" }).Build();
        Assert.True(DatabaseConfiguration.UsePostgreSql(config));
        Assert.Equal("postgres://deployment/db", DatabaseConfiguration.ResolveConnectionString(config));
    }

    [Fact]
    public void EmptyDatabaseUrlFallsBackToConnectionString()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["DATABASE_URL"] = " ", ["ConnectionStrings:DefaultConnection"] = "local" }).Build();
        Assert.True(DatabaseConfiguration.UsePostgreSql(config));
        Assert.Equal("local", DatabaseConfiguration.ResolveConnectionString(config));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PlanUpdatesAreScopedToOwnAgency(bool ownAgency)
    {
        using var db = Database();
        var agencyId = Guid.NewGuid();
        var target = new Agency { Id = ownAgency ? agencyId : Guid.NewGuid() };
        db.Agencies.Add(target);
        await db.SaveChangesAsync();
        var ctx = Context("Agency", "UpdatePlansAndContacts", "PUT", new() {
            ["id"] = target.Id, ["userAgencyId"] = target.Id }, agencyId);
        Assert.Equal(ownAgency, await Run(db, ctx));
        Assert.Equal(agencyId, ctx.ActionArguments["userAgencyId"]);
    }

    [Fact]
    public async Task NotificationReadCannotUseAnotherAgencysIdentity()
    {
        using var db = Database();
        var notification = new Notification { AgencyId = Guid.NewGuid() };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        var ctx = Context("Notification", "MarkAsRead", "GET", new() { ["id"] = notification.Id }, Guid.NewGuid());
        Assert.False(await Run(db, ctx));
        Assert.IsType<ForbidResult>(ctx.Result);
    }
}
