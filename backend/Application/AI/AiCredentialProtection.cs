using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cdsqg.Application.AI;

public static class AiCredentialProtection
{
    // Keep this discriminator unchanged so existing encrypted API keys remain readable.
    public const string ApplicationName = "CDSQG.AiCredentials";

    public static IServiceCollection AddAiCredentialProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var protection = services.AddDataProtection().SetApplicationName(ApplicationName).PersistKeysToDbContext<AppDbContext>();
        // Optional independent protection for keyring material in database backups.
        // Keep the PFX in deployment secrets, never in appsettings or the database.
        var encodedCertificate = configuration["AiData:KeyEncryptionCertificateBase64"];
        if (!string.IsNullOrWhiteSpace(encodedCertificate))
        {
            try
            {
                var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(encodedCertificate),
                    configuration["AiData:KeyEncryptionCertificatePassword"], X509KeyStorageFlags.EphemeralKeySet);
                if (!certificate.HasPrivateKey) throw new CryptographicException();
                protection.ProtectKeysWithCertificate(certificate);
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException)
            {
                // Do not include certificate bytes/passwords in exceptions or logs.
                throw new InvalidOperationException("AI key protection certificate is invalid or has no private key.");
            }
        }
        return services;
    }

    public static async Task<int> ImportLegacyKeysAsync(AppDbContext db, IConfiguration configuration, string contentRoot, CancellationToken ct = default)
    {
        var configuredDirectory = configuration["AiData:ProtectionKeyDirectory"];
        var directory = string.IsNullOrWhiteSpace(configuredDirectory) ? Path.Combine(contentRoot, "App_Data", "ai-keys") : configuredDirectory;
        if (!Directory.Exists(directory)) return 0;
        var elements = new FileSystemXmlRepository(new DirectoryInfo(directory), NullLoggerFactory.Instance).GetAllElements();
        if (elements.Count == 0) return 0;
        // Serialize imports from simultaneous application starts. Existing DB keys
        // and legacy files are never removed or overwritten.
        await using var transaction = db.Database.IsNpgsql() ? await db.Database.BeginTransactionAsync(ct) : null;
        if (transaction != null) await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(26292510)", ct);
        var names = (await db.DataProtectionKeys.Select(key => key.FriendlyName).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
        var imported = 0;
        foreach (var element in elements)
        {
            var xml = element.ToString(SaveOptions.DisableFormatting);
            var name = element.Name.LocalName == "key" && element.Attribute("id") != null
                ? "key-" + element.Attribute("id")!.Value
                : "legacy-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(xml)));
            if (!names.Add(name)) continue;
            db.DataProtectionKeys.Add(new DataProtectionKey { FriendlyName = name, Xml = xml });
            imported++;
        }
        if (imported > 0) await db.SaveChangesAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
        return imported;
    }
}
