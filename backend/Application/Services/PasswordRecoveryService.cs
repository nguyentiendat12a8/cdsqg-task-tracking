using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

public interface IRecoveryEmailSender
{
    Task SendAsync(string email, string resetUrl);
}

public sealed class SmtpRecoveryEmailSender(IConfiguration config) : IRecoveryEmailSender
{
    public async Task SendAsync(string email, string resetUrl)
    {
        var host = config["Smtp:Host"] ?? throw new InvalidOperationException("SMTP is not configured.");
        using var client = new SmtpClient(host, config.GetValue("Smtp:Port", 587)) {
            EnableSsl = config.GetValue("Smtp:EnableSsl", true),
            Credentials = new NetworkCredential(config["Smtp:Username"], config["Smtp:Password"]),
            Timeout = 15000
        };
        using var message = new MailMessage(config["Smtp:From"] ?? throw new InvalidOperationException("SMTP From is missing."), email) {
            Subject = "Đặt lại mật khẩu hệ thống theo dõi chiến lược",
            Body = $"Bạn đã yêu cầu đặt lại mật khẩu. Liên kết chỉ dùng một lần và hết hạn sau 20 phút:\n{resetUrl}\nNếu không yêu cầu, hãy bỏ qua email này."
        };
        await client.SendMailAsync(message);
    }
}

public sealed class PasswordRecoveryService(AppDbContext db, IPasswordHasher hasher,
    IRecoveryEmailSender emailSender, IConfiguration config, ILogger<PasswordRecoveryService> logger)
{
    public async Task RequestAsync(string input)
    {
        var q = input.Trim().ToLowerInvariant();
        var matches = await db.Users.Where(u => u.IsActive && (u.Username.ToLower() == q || u.Email.ToLower() == q)).Take(2).ToListAsync();
        var user = matches.Count == 1 ? matches[0] : null;
        var baseUrl = config["PasswordRecovery:FrontendUrl"];
        if (user == null || string.IsNullOrWhiteSpace(user.Email) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != "https" && !uri.IsLoopback)) return;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash = Hash(token);
        user.PasswordResetTokenHash = hash;
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddMinutes(20);
        await db.SaveChangesAsync();
        try { await emailSender.SendAsync(user.Email, $"{baseUrl!.TrimEnd('/')}/#reset-password?token={token}"); }
        catch (Exception ex)
        {
            // Do not log tokens, recipient addresses, SMTP credentials or message body.
            logger.LogWarning("Password recovery email delivery failed ({ExceptionType}).", ex.GetType().Name);
            await db.Users.Where(u => u.Id == user.Id && u.PasswordResetTokenHash == hash)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.PasswordResetTokenHash, (string?)null)
                    .SetProperty(u => u.PasswordResetExpiresAt, (DateTime?)null));
        }
    }

    public async Task<bool> ResetAsync(string token, string password)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(password)) return false;
        if (token.Length != 64 || !token.All(Uri.IsHexDigit) || password.Length < 12 || password.Length > 128) return false;
        var hash = Hash(token);
        var now = DateTime.UtcNow;
        var passwordHash = hasher.HashPassword(password);
        var stamp = Guid.NewGuid().ToString("N");
        // One conditional UPDATE consumes the token atomically, including concurrent requests.
        return await db.Users.Where(u => u.IsActive && u.PasswordResetTokenHash == hash && u.PasswordResetExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PasswordHash, passwordHash)
                .SetProperty(u => u.SecurityStamp, stamp)
                .SetProperty(u => u.PasswordResetTokenHash, (string?)null)
                .SetProperty(u => u.PasswordResetExpiresAt, (DateTime?)null)) == 1;
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
