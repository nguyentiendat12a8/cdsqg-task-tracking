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
    options.JsonSerializerOptions.Converters.Add(new DateTimeUtcJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDateTimeUtcJsonConverter());
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

app.UseCors("AllowFrontend");

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
        EnsureSampleFilesExist(app.Environment);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Database Initialization notice: {ex.Message}");
    }
}

void EnsureSampleFilesExist(IWebHostEnvironment env)
{
    try
    {
        string webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        string uploadsDir = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        string[] sampleFiles = new[]
        {
            "1266_QD_TTg.pdf",
            "Phu_luc_Chi_tieu.pdf",
            "ND_15_2026.pdf",
            "TT_08_2026.pdf",
            "TT_42_BCA.pdf"
        };

        string pdfTemplate = @"%PDF-1.4
1 0 obj <</Type /Catalog /Pages 2 0 R>> endobj
2 0 obj <</Type /Pages /Kids [3 0 R] /Count 1>> endobj
3 0 obj <</Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R>> endobj
4 0 obj <</Type /Font /Subtype /Type1 /BaseFont /Helvetica>> endobj
5 0 obj <</Length 73>> stream
BT
/F1 14 Tf
72 720 TD
(VAN BAN QUY PHAM PHAP LUAT - TAI LIEU COMPONENT SAMPLE) Tj
ET
endstream endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000244 00000 n 
0000000318 00000 n 
trailer <</Size 6 /Root 1 0 R>>
startxref
441
%%EOF";

        byte[] pdfBytes = System.Text.Encoding.UTF8.GetBytes(pdfTemplate);

        foreach (var file in sampleFiles)
        {
            string filePath = Path.Combine(uploadsDir, file);
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, pdfBytes);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] EnsureSampleFilesExist error: {ex.Message}");
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
            ALTER TABLE ""Agencies"" ADD COLUMN IF NOT EXISTS ""ContactPersons"" text NULL;
            ALTER TABLE ""Agencies"" ADD COLUMN IF NOT EXISTS ""PlanFiles"" text NULL;
            UPDATE ""Agencies"" SET ""ContactPersons"" = '[]' WHERE ""ContactPersons"" IS NULL OR ""ContactPersons"" = '';
            UPDATE ""Agencies"" SET ""PlanFiles"" = '[]' WHERE ""PlanFiles"" IS NULL OR ""PlanFiles"" = '';
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
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""Deliverables"" jsonb NOT NULL DEFAULT '[]'::jsonb;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""AgencyDeliverables"" jsonb NOT NULL DEFAULT '{{}}'::jsonb;
            ALTER TABLE ""GoalTaskItems"" ADD COLUMN IF NOT EXISTS ""AssignedAgencyId"" uuid NULL;

            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""AgencyId"" uuid NULL;
            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""Deliverables"" jsonb NOT NULL DEFAULT '[]'::jsonb;
            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""ApprovalStatus"" integer NOT NULL DEFAULT 1;
            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" text NULL;
            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""ApprovedBy"" text NULL;
            ALTER TABLE ""ProgressLogs"" ADD COLUMN IF NOT EXISTS ""ApprovedAt"" timestamp without time zone NULL;

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

        // 5. Ensure TaskUrgeLogs columns
        string sqlTaskUrgeLogs = @"
            ALTER TABLE ""TaskUrgeLogs"" ADD COLUMN IF NOT EXISTS ""RecipientsSummary"" text NULL;
            ALTER TABLE ""TaskUrgeLogs"" ADD COLUMN IF NOT EXISTS ""LeadAgencyId"" uuid NULL;
            ALTER TABLE ""TaskUrgeLogs"" ADD COLUMN IF NOT EXISTS ""Title"" text NULL;
            UPDATE ""TaskUrgeLogs"" SET ""RecipientsSummary"" = '' WHERE ""RecipientsSummary"" IS NULL;
        ";
        db.Database.ExecuteSqlRaw(sqlTaskUrgeLogs);

        // 6. Ensure DataImportLogs columns
        string sqlDataImportLogs = @"
            ALTER TABLE ""DataImportLogs"" ADD COLUMN IF NOT EXISTS ""AgencyId"" uuid NULL;
        ";
        db.Database.ExecuteSqlRaw(sqlDataImportLogs);

        // 7. Ensure AgencyTaskExecutions table
        string sqlAgencyTaskExecutions = @"
            CREATE TABLE IF NOT EXISTS ""AgencyTaskExecutions"" (
                ""Id"" uuid NOT NULL CONSTRAINT ""PK_AgencyTaskExecutions"" PRIMARY KEY,
                ""GoalTaskId"" uuid NOT NULL CONSTRAINT ""FK_AgencyTaskExecutions_GoalTaskItems_GoalTaskId"" REFERENCES ""GoalTaskItems"" (""Id"") ON DELETE CASCADE,
                ""AgencyId"" uuid NOT NULL CONSTRAINT ""FK_AgencyTaskExecutions_Agencies_AgencyId"" REFERENCES ""Agencies"" (""Id"") ON DELETE CASCADE,
                ""CalculatedStatus"" text NOT NULL,
                ""LatestProgressValue"" numeric NULL,
                ""LatestQualitativeStatus"" text NULL,
                ""CompletionPercentage"" numeric NOT NULL DEFAULT 0,
                ""Deliverables"" jsonb NOT NULL DEFAULT '[]'::jsonb,
                ""SummaryNotes"" text NULL,
                ""AttachmentFileUrls"" jsonb NOT NULL DEFAULT '[]'::jsonb,
                ""LastReportedAt"" timestamp without time zone NULL,
                ""LastReportedBy"" text NULL,
                ""CreatedAt"" timestamp without time zone NOT NULL,
                ""UpdatedAt"" timestamp without time zone NOT NULL
            );
            ALTER TABLE ""AgencyTaskExecutions"" ADD COLUMN IF NOT EXISTS ""SummaryNotes"" text NULL;
            ALTER TABLE ""AgencyTaskExecutions"" ADD COLUMN IF NOT EXISTS ""AttachmentFileUrls"" jsonb NOT NULL DEFAULT '[]'::jsonb;
            ALTER TABLE ""AgencyTaskExecutions"" ADD COLUMN IF NOT EXISTS ""AssignedAgencyId"" uuid NULL;
            ALTER TABLE ""AgencyTaskExecutions"" ADD COLUMN IF NOT EXISTS ""ApprovalStatus"" integer NOT NULL DEFAULT 1;
            ALTER TABLE ""AgencyTaskExecutions"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" text NULL;
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AgencyTaskExecutions_GoalTaskId_AgencyId"" ON ""AgencyTaskExecutions"" (""GoalTaskId"", ""AgencyId"");
        ";
        db.Database.ExecuteSqlRaw(sqlAgencyTaskExecutions);

        // 8. Ensure LegalDocuments table
        string sqlLegalDocuments = @"
            CREATE TABLE IF NOT EXISTS ""LegalDocuments"" (
                ""Id"" uuid NOT NULL CONSTRAINT ""PK_LegalDocuments"" PRIMARY KEY,
                ""Code"" text NOT NULL,
                ""Title"" text NOT NULL,
                ""DocumentType"" text NOT NULL,
                ""IssuingAgencyId"" uuid NULL,
                ""IssuingAgencyName"" text NULL,
                ""DraftingAgencyId"" uuid NULL,
                ""DraftingAgencyName"" text NULL,
                ""SignerName"" text NULL,
                ""SignerTitle"" text NULL,
                ""IssuedDate"" timestamp without time zone NULL,
                ""EffectiveDate"" timestamp without time zone NULL,
                ""EffectStatus"" text NOT NULL,
                ""Field"" text NULL,
                ""Scope"" text NULL,
                ""AttachmentsJson"" text NULL,
                ""Notes"" text NULL,
                ""CreatedByAgencyId"" uuid NULL,
                ""CreatedByUserId"" uuid NULL,
                ""CreatedAt"" timestamp without time zone NOT NULL,
                ""UpdatedAt"" timestamp without time zone NOT NULL
            );
            ALTER TABLE ""LegalDocuments"" ADD COLUMN IF NOT EXISTS ""CreatedByAgencyId"" uuid NULL;
            ALTER TABLE ""LegalDocuments"" ADD COLUMN IF NOT EXISTS ""CreatedByUserId"" uuid NULL;
        ";
        db.Database.ExecuteSqlRaw(sqlLegalDocuments);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] EnsureDatabaseSchemaUpdated error: {ex.Message}");
    }
}

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".pdf"] = "application/pdf";
contentTypeProvider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
contentTypeProvider.Mappings[".doc"] = "application/msword";
contentTypeProvider.Mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
contentTypeProvider.Mappings[".xls"] = "application/vnd.ms-excel";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        ctx.Context.Response.Headers["Content-Disposition"] = "inline";
    }
});
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
    // 1. Seed Reference Special Agencies Dictionary if missing
    var specialItems = new[]
    {
        new { Id = Guid.Parse("00000000-0000-0000-0000-000000009999"), Code = "ALL_AGENCIES", Name = "Các bộ, ngành, địa phương" },
        new { Id = Guid.Parse("00000000-0000-0000-0000-000000009998"), Code = "ALL_MINISTRIES", Name = "Các bộ, ngành chủ quản cơ sở dữ liệu" },
        new { Id = Guid.Parse("00000000-0000-0000-0000-000000009997"), Code = "ALL_PROVINCES", Name = "Các địa phương" },
        new { Id = Guid.Parse("00000000-0000-0000-0000-000000009996"), Code = "ALL_PROVINCES_UBND", Name = "UBND tỉnh, thành phố trực thuộc trung ương" },
        new { Id = Guid.Parse("00000000-0000-0000-0000-000000009995"), Code = "ALL_MINISTRIES_DIRECT", Name = "Các bộ, ngành" }
    };

    foreach (var spec in specialItems)
    {
        var existing = db.Agencies.FirstOrDefault(a => a.Code == spec.Code);
        if (existing == null)
        {
            db.Agencies.Add(new Agency
            {
                Id = spec.Id,
                Code = spec.Code,
                Name = spec.Name,
                Type = AgencyTypeEnum.Special,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (existing.Type != AgencyTypeEnum.Special)
        {
            existing.Type = AgencyTypeEnum.Special;
        }
    }
    db.SaveChanges();

    // Set IsGeneralTask = true for items assigned to any of the 5 special agencies
    var specialAgencies = db.Agencies.Where(a => a.Code == "ALL_AGENCIES" || a.Code == "ALL_MINISTRIES" || a.Code == "ALL_PROVINCES" || a.Code == "ALL_PROVINCES_UBND" || a.Code == "ALL_MINISTRIES_DIRECT").Select(a => a.Id).ToList();
    if (specialAgencies.Count > 0)
    {
        var itemsToSetGeneral = db.GoalTaskItems.Where(i => !i.IsGeneralTask && specialAgencies.Contains(i.LeadAgencyId)).ToList();
        if (itemsToSetGeneral.Count > 0)
        {
            foreach (var item in itemsToSetGeneral)
            {
                item.IsGeneralTask = true;
            }
            db.SaveChanges();
        }
    }

    var specialCodes = specialItems.Select(s => s.Code).ToList();
    if (!db.Agencies.Any(a => !specialCodes.Contains(a.Code)))
    {
        var btttt = new Agency { Id = Guid.NewGuid(), Code = "BTTTT", Name = "Bộ Thông tin và Truyền thông", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var bca = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var bkhdt = new Agency { Id = Guid.NewGuid(), Code = "BKHĐT", Name = "Bộ Kế hoạch và Đầu tư", Type = AgencyTypeEnum.Ministry, CreatedAt = DateTime.UtcNow };
        var tphcm = new Agency { Id = Guid.NewGuid(), Code = "TPHCM", Name = "UBND TP. Hồ Chí Minh", Type = AgencyTypeEnum.Province, CreatedAt = DateTime.UtcNow };
        db.Agencies.AddRange(btttt, bca, bkhdt, tphcm);
        db.SaveChanges();
    }

    // 2. Seed Reference Units Dictionary if missing
    if (!db.Units.Any(u => u.Name == "%"))
    {
        db.Units.Add(new UnitDictionary { Id = Guid.NewGuid(), Code = "PERCENT", Name = "%", DataType = UnitDataTypeEnum.Decimal });
    }
    if (!db.Units.Any(u => u.Name == "Số lượng"))
    {
        db.Units.Add(new UnitDictionary { Id = Guid.NewGuid(), Code = "QTY", Name = "Số lượng", DataType = UnitDataTypeEnum.Decimal });
    }
    db.SaveChanges();

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

    // Fix existing Level 3 progress logs that were incorrectly marked as Approved
    var subAgencyIds = db.Agencies.Where(a => a.ParentId.HasValue).Select(a => a.Id).ToList();
    var assignedTaskIds = db.GoalTaskItems.Where(i => i.AssignedAgencyId.HasValue).Select(i => i.Id).ToList();
    var misMarkedLogs = db.ProgressLogs
        .Where(p => p.ApprovalStatus == ApprovalStatusEnum.Approved &&
                    ((p.AgencyId.HasValue && subAgencyIds.Contains(p.AgencyId.Value)) || assignedTaskIds.Contains(p.GoalTaskId)))
        .ToList();

    foreach (var log in misMarkedLogs)
    {
        log.ApprovalStatus = ApprovalStatusEnum.Pending;
        log.ApprovedBy = null;
        log.ApprovedAt = null;
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
        var primaryGoals = group.Where(i => i.ItemType == ItemTypeEnum.Goal && !i.ParentId.HasValue).OrderBy(i => i.CreatedAt).ToList();
        var primaryTasks = group.Where(i => i.ItemType == ItemTypeEnum.Task && !i.ParentId.HasValue).OrderBy(i => i.CreatedAt).ToList();

        // Check for duplicates or invalid codes
        var goalCodes = primaryGoals.Select(g => g.Code).ToList();
        var taskCodes = primaryTasks.Select(t => t.Code).ToList();

        bool renumberGoals = goalCodes.Count != goalCodes.Distinct().Count() || primaryGoals.Any(g => string.IsNullOrWhiteSpace(g.Code) || !g.Code.StartsWith("MT-"));
        bool renumberTasks = taskCodes.Count != taskCodes.Distinct().Count() || primaryTasks.Any(t => string.IsNullOrWhiteSpace(t.Code) || !t.Code.StartsWith("NV-"));

        if (renumberGoals)
        {
            int goalIdx = 1;
            foreach (var item in primaryGoals)
            {
                var newCode = $"MT-{goalIdx:D2}";
                if (item.Code != newCode)
                {
                    item.Code = newCode;
                    updated = true;
                }
                goalIdx++;
            }
        }

        if (renumberTasks)
        {
            int taskIdx = 1;
            foreach (var item in primaryTasks)
            {
                var newCode = $"NV-{taskIdx:D2}";
                if (item.Code != newCode)
                {
                    item.Code = newCode;
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

public class DateTimeUtcJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str)) return DateTime.MinValue;
        if (DateTime.TryParse(str, out var dt))
        {
            return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
        }
        return DateTime.MinValue;
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
    {
        var utcValue = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

public class NullableDateTimeUtcJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
{
    public override DateTime? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str)) return null;
        if (DateTime.TryParse(str, out var dt))
        {
            return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
        }
        return null;
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime? value, System.Text.Json.JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }
        var utcValue = value.Value.Kind == DateTimeKind.Utc ? value.Value : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
