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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cdsqg.Api.Security;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

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
builder.Services.AddScoped<ApiPermissionFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<ApiPermissionFilter>()).AddJsonOptions(options =>
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
string connectionString = DatabaseConfiguration.ResolveConnectionString(builder.Configuration);
bool usePostgreSql = DatabaseConfiguration.UsePostgreSql(builder.Configuration);

connectionString = ConvertPostgresConnectionString(connectionString);

if (usePostgreSql && !string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    if (!builder.Environment.IsDevelopment() || !builder.Configuration.GetValue<bool>("AllowInMemoryDatabase"))
        throw new InvalidOperationException("Configure PostgreSQL. InMemory requires Development and AllowInMemoryDatabase=true.");
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
builder.Services.AddScoped<IRecoveryEmailSender, SmtpRecoveryEmailSender>();
builder.Services.AddScoped<PasswordRecoveryService>();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.AddPolicy("password-recovery", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0
        }));
});

// JWT Authentication Configuration
string? jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Configure Jwt:SecretKey (environment variable Jwt__SecretKey) before starting in production.");
    jwtSecret = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    builder.Configuration["Jwt:SecretKey"] = jwtSecret;
}
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Jwt:SecretKey must contain at least 32 UTF-8 bytes.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var principal = context.Principal;
            if (!Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            { context.Fail("Invalid user."); return; }
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.IsActive || principal?.FindFirstValue("SecurityStamp") != user.SecurityStamp || principal?.FindFirstValue(ClaimTypes.Role) != user.Role.ToString()
                || principal?.FindFirstValue("AgencyId") != (user.AgencyId?.ToString() ?? ""))
                context.Fail("Account is inactive or permissions have changed.");
        }
    };
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
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
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
            EnsurePostgresSchemaUpToDate(context);
        }
        SeedInitialData(context, hasher);
        NormalizeGoalTaskItemCodes(context);
        EnsureSampleFilesExist(app.Environment);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Database initialization failed.", ex);
    }
}

void EnsurePostgresSchemaUpToDate(AppDbContext context)
{
    // Never serve requests against a partially migrated schema.
    context.Database.Migrate();
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
app.UseRateLimiter();
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
        // Legacy data can have multiple agencies sharing a display name. Prefer the
        // unique code so seeding never renames another record to an existing code.
        var existing = db.Agencies.FirstOrDefault(a => a.Code == spec.Code)
            ?? db.Agencies.FirstOrDefault(a => a.Name == spec.Name);
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
        else
        {
            existing.Code = spec.Code;
            existing.Name = spec.Name;
            existing.Type = AgencyTypeEnum.Special;
        }
    }
    db.SaveChanges();

    // Restore proper types for regular agencies if they were marked as Special
    var specCodes = new HashSet<string> { "ALL_AGENCIES", "ALL_MINISTRIES", "ALL_PROVINCES", "ALL_PROVINCES_UBND", "ALL_MINISTRIES_DIRECT" };
    var regularAgencies = db.Agencies.Where(a => !specCodes.Contains(a.Code)).ToList();
    foreach (var ag in regularAgencies)
    {
        if (ag.Type == AgencyTypeEnum.Special)
        {
            string nameLower = (ag.Name ?? "").ToLower();
            if (nameLower.StartsWith("ubnd") || nameLower.StartsWith("tỉnh") || nameLower.StartsWith("thành phố") || nameLower.StartsWith("tp."))
            {
                ag.Type = AgencyTypeEnum.Province;
            }
            else if (nameLower.StartsWith("bộ") || nameLower.StartsWith("bảo hiểm") || nameLower.StartsWith("ngân hàng") || nameLower.StartsWith("viện") || nameLower.StartsWith("đài") || nameLower.StartsWith("thông tấn"))
            {
                ag.Type = AgencyTypeEnum.Ministry;
            }
            else
            {
                ag.Type = AgencyTypeEnum.Other;
            }
        }
    }
    db.SaveChanges();

    // Enforce IsGeneralTask = true ONLY for items assigned to special general agencies (code starting with ALL_)
    var specialAgencies = db.Agencies.Where(a => a.Code.StartsWith("ALL_")).Select(a => a.Id).ToList();
    if (specialAgencies.Count > 0)
    {
        var itemsToSetGeneral = db.GoalTaskItems.Where(i => !i.IsGeneralTask && specialAgencies.Contains(i.LeadAgencyId)).ToList();
        foreach (var item in itemsToSetGeneral)
        {
            item.IsGeneralTask = true;
        }

        var itemsToUnsetGeneral = db.GoalTaskItems.Where(i => i.IsGeneralTask && !specialAgencies.Contains(i.LeadAgencyId)).ToList();
        foreach (var item in itemsToUnsetGeneral)
        {
            item.IsGeneralTask = false;
        }
        db.SaveChanges();
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

    // Approval decisions belong to the approval endpoints; startup must preserve them.

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
        var primaryGoals = group.Where(i => i.ItemType == ItemTypeEnum.Goal).OrderBy(i => i.CreatedAt).ToList();
        var primaryTasks = group.Where(i => i.ItemType == ItemTypeEnum.Task).OrderBy(i => i.CreatedAt).ToList();

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
