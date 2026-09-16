using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Cdsqg.Infrastructure.Data;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using System;
using System.Linq;
using System.Text;

// Enable Npgsql Legacy Timestamp Behavior for seamless DateTime support in PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Ensure wwwroot directory exists for static file serving
string wwwrootFolder = System.IO.Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!System.IO.Directory.Exists(wwwrootFolder))
{
    System.IO.Directory.CreateDirectory(wwwrootFolder);
}
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(wwwrootFolder, "uploads", "documents"));
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(wwwrootFolder, "uploads", "evidence"));
builder.Environment.WebRootPath = wwwrootFolder;

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read PostgreSQL flag from appsettings.json or environment variables
bool usePostgreSql = builder.Configuration.GetValue<bool>("UsePostgreSQL") || 
                     !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"));

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                         ?? builder.Configuration["DATABASE_URL"] 
                         ?? string.Empty;

connectionString = ConvertPostgresConnectionString(connectionString);

if (usePostgreSql && !string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("CdsqgTaskTrackingDb"));
}

// Helper to convert postgres:// URI format to Npgsql connection string format
static string ConvertPostgresConnectionString(string connStr)
{
    if (string.IsNullOrWhiteSpace(connStr)) return connStr;
    if (connStr.StartsWith("postgres://") || connStr.StartsWith("postgresql://"))
    {
        try
        {
            var uri = new Uri(connStr);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }
        catch
        {
            return connStr;
        }
    }
    return connStr;
}

// Register Application & Auth Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPlanningService, PlanningService>();
builder.Services.AddScoped<IExecutionService, ExecutionService>();

// JWT Authentication Configuration
string jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "CdsqgNationalDigitalTransformationSecretKey2026MustBeAtLeast32BytesLong!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CdsqgSystem",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CdsqgClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// CORS Policy: Allow Vue Dev Server (localhost:5173, 5174) and wildcard for local dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure Database Clean Recreation & Seed Initial Data (Fail-safe wrapper)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        if (usePostgreSql && !string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                context.Database.EnsureCreated();
                EnsureDatabaseSchemaUpdated(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] PostgreSQL EnsureCreated notice: {ex.Message}");
            }
        }
        SeedInitialData(context, hasher);
        NormalizeGoalTaskItemCodes(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Database Initialization notice: {ex.Message}");
    }
}

void EnsureDatabaseSchemaUpdated(AppDbContext db)
{
    try
    {
        // 1. Ensure Users table
        string sqlUsers = @"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id"" uuid NOT NULL CONSTRAINT ""PK_Users"" PRIMARY KEY,
                ""Username"" text NOT NULL,
                ""PasswordHash"" text NOT NULL,
                ""FullName"" text NOT NULL,
                ""Email"" text NOT NULL,
                ""Role"" text NOT NULL,
                ""AgencyId"" uuid NULL CONSTRAINT ""FK_Users_Agencies_AgencyId"" REFERENCES ""Agencies"" (""Id"") ON DELETE SET NULL,
                ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                ""CreatedAt"" timestamp without time zone NOT NULL,
                ""LastLoginAt"" timestamp without time zone NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Username"" ON ""Users"" (""Username"");
        ";
        db.Database.ExecuteSqlRaw(sqlUsers);

        // 2. Ensure Agencies columns
        string sqlAgencies = @"
            ALTER TABLE ""Agencies"" ADD COLUMN IF NOT EXISTS ""ParentId"" uuid NULL;
        ";
        db.Database.ExecuteSqlRaw(sqlAgencies);

        // 3. Ensure GoalTaskItems columns
        string sqlGoalTaskItems = @"
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""ParentId"" uuid NULL;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""StartDate"" timestamp without time zone NULL;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""DueDate"" timestamp without time zone NULL;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""IsGeneralTask"" boolean NOT NULL DEFAULT FALSE;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""Section"" text NULL;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""Group"" text NULL;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""IsOngoing"" boolean NOT NULL DEFAULT FALSE;
            UPDATE ""GoalTaskItems"" SET ""Section"" = '' WHERE ""Section"" IS NULL;
            UPDATE ""GoalTaskItems"" SET ""Group"" = '' WHERE ""Group"" IS NULL;
        ";
        db.Database.ExecuteSqlRaw(sqlGoalTaskItems);

        // 4. Ensure Notifications table
        string sqlNotifications = @"
            CREATE TABLE IF NOT EXISTS ""Notifications"" (
                ""Id"" uuid NOT NULL CONSTRAINT ""PK_Notifications"" PRIMARY KEY,
                ""UserId"" uuid NULL CONSTRAINT ""FK_Notifications_Users_UserId"" REFERENCES ""Users"" (""Id"") ON DELETE CASCADE,
                ""AgencyId"" uuid NULL CONSTRAINT ""FK_Notifications_Agencies_AgencyId"" REFERENCES ""Agencies"" (""Id"") ON DELETE CASCADE,
                ""Title"" text NOT NULL,
                ""Message"" text NOT NULL,
                ""Type"" text NOT NULL,
                ""IsRead"" boolean NOT NULL DEFAULT FALSE,
                ""LinkUrl"" text NULL,
                ""CreatedAt"" timestamp without time zone NOT NULL
            );
        ";
        db.Database.ExecuteSqlRaw(sqlNotifications);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] EnsureDatabaseSchemaUpdated error: {ex.Message}");
    }
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "National Digital Transformation Tracking API v1");
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

void SeedInitialData(AppDbContext db, IPasswordHasher hasher)
{
    // 1. Seed Reference Agencies Dictionary if missing
    if (!db.Agencies.Any())
    {
        var btttt = new Agency { Id = Guid.NewGuid(), Code = "BTTTT", Name = "Bộ Thông tin và Truyền thông", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var bca = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var bkhdt = new Agency { Id = Guid.NewGuid(), Code = "BKHĐT", Name = "Bộ Kế hoạch và Đầu tư", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var tphcm = new Agency { Id = Guid.NewGuid(), Code = "TPHCM", Name = "UBND TP. Hồ Chí Minh", Type = AgencyTypeEnum.Province, CreatedAt = DateTime.UtcNow };
        db.Agencies.AddRange(btttt, bca, bkhdt, tphcm);
    }

    // 2. Seed Reference Units Dictionary if missing
    if (!db.Units.Any())
    {
        var pctUnit = new UnitDictionary { Id = Guid.NewGuid(), Code = "PERCENT", Name = "%", DataType = UnitDataTypeEnum.Decimal };
        var docUnit = new UnitDictionary { Id = Guid.NewGuid(), Code = "DOC", Name = "Văn bản", DataType = UnitDataTypeEnum.Text_Status };
        db.Units.AddRange(pctUnit, docUnit);
    }

    // 3. Seed Default Admin User if no users exist
    if (!db.Users.Any())
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.HashPassword("adminpassword"),
            FullName = "Quản trị viên Hệ thống",
            Email = "admin@cdsqg.gov.vn",
            Role = UserRoleEnum.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(adminUser);
    }

    // 4. Seed Decision 1266 Document Container if missing
    var doc1266Id = Guid.Parse("12660000-0000-0000-0000-000000001266");
    var doc1266 = db.Documents.FirstOrDefault(d => d.Id == doc1266Id || d.DocumentNumber == "1266/QĐ-TTg");
    if (doc1266 == null)
    {
        doc1266 = new Document
        {
            Id = doc1266Id,
            DocumentNumber = "1266/QĐ-TTg",
            Name = "Quyết định số 1266/QĐ-TTg ngày 14/07/2026 của Thủ tướng Chính phủ",
            Summary = "Hệ thống theo dõi nhiệm vụ được giao tại Quyết định số 1266/QĐ-TTg",
            Signer = "Thủ tướng Chính phủ",
            IssueDate = new DateTime(2026, 7, 14),
            StartYear = 2026,
            EndYear = 2030,
            CreatedAt = DateTime.UtcNow
        };
        db.Documents.Add(doc1266);
    }

    var targetDocId = doc1266.Id;

    // 5. Ensure existing GoalTaskItems are assigned to Decision 1266 Document
    var orphanItems = db.GoalTaskItems.Where(i => i.DocumentId != targetDocId).ToList();
    if (orphanItems.Any())
    {
        foreach (var item in orphanItems)
        {
            item.DocumentId = targetDocId;
        }
    }

    db.SaveChanges();
}

void NormalizeGoalTaskItemCodes(AppDbContext db)
{
    var allItems = db.GoalTaskItems.ToList();
    if (!allItems.Any()) return;

    var groupedByDoc = allItems.GroupBy(i => i.DocumentId);
    bool updated = false;

    foreach (var group in groupedByDoc)
    {
        int goalIdx = 1;
        int taskIdx = 1;

        foreach (var item in group.OrderBy(i => i.CreatedAt))
        {
            bool isMissingDigit = string.IsNullOrWhiteSpace(item.Code) || !item.Code.Any(char.IsDigit);

            if (item.ItemType == ItemTypeEnum.Goal)
            {
                if (isMissingDigit)
                {
                    item.Code = $"MT-{goalIdx:D2}";
                    updated = true;
                }
                goalIdx++;
            }
            else
            {
                if (isMissingDigit)
                {
                    item.Code = $"NV-{taskIdx:D2}";
                    updated = true;
                }
                taskIdx++;
            }
        }
    }

    if (updated)
    {
        db.SaveChanges();
    }
}
