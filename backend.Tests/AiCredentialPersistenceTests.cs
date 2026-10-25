using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Cdsqg.Application.AI;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;

public class AiCredentialPersistenceTests
{
    private const string Purpose = "CDSQG.AiProvider.ApiKey.v1";
    private static AiProviderInput Input(string key = "", bool enabled = true, bool remove = false) => new(enabled, "https://example.com/v1", "fixture-model", key, remove);
    private static AppDbContext MemoryDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static AiSettingsStore Store(AppDbContext db, IDataProtectionProvider provider) => new(db, provider, Options.Create(new AiDataOptions()));

    [Fact]
    public async Task MissingKeyringIsReportedWithoutExposingOrOverwritingCiphertext()
    {
        await using var db = MemoryDb();
        var oldProvider = new EphemeralDataProtectionProvider();
        await Store(db, oldProvider).SaveAsync(Input("fixture-original"), "fixture", default);
        var encrypted = (await db.AiProviderConfigurations.SingleAsync()).ProtectedApiKey;
        var unavailable = Store(db, new EphemeralDataProtectionProvider());
        var publicJson = JsonSerializer.Serialize(await unavailable.PublicAsync(default));
        Assert.Contains("Unreadable", publicJson);
        Assert.DoesNotContain("fixture-original", publicJson);
        Assert.DoesNotContain(encrypted, publicJson);
        Assert.False((await unavailable.GetAsync()).Ready);
        Assert.Equal("", (await unavailable.GetAsync()).ApiKey);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => unavailable.SaveAsync(Input(enabled: false), "fixture", default));
        Assert.Equal(AiSettingsStore.UnreadableKeyMessage, error.Message);
        Assert.Equal(encrypted, (await db.AiProviderConfigurations.SingleAsync()).ProtectedApiKey);
        // Re-entering a key deliberately repairs the credential, without changing other data.
        await unavailable.SaveAsync(Input("fixture-replacement"), "fixture", default);
        Assert.Equal("fixture-replacement", (await unavailable.GetAsync()).ApiKey);
    }

    [Fact]
    public async Task EmptyInputPreservesReadableKeyAndRemovalRequiresExplicitIntent()
    {
        await using var db = MemoryDb();
        var store = Store(db, new EphemeralDataProtectionProvider());
        await store.SaveAsync(Input("fixture-original"), "fixture", default);
        await store.SaveAsync(Input(), "fixture", default);
        Assert.Equal("fixture-original", (await store.GetAsync()).ApiKey);
        var unavailable = Store(db, new EphemeralDataProtectionProvider());
        await unavailable.SaveAsync(Input(enabled: false, remove: true), "fixture", default);
        Assert.Equal("", (await db.AiProviderConfigurations.SingleAsync()).ProtectedApiKey);
        Assert.Contains("Missing", JsonSerializer.Serialize(await unavailable.PublicAsync(default)));
    }

    [Fact]
    public void InvalidCertificateFailsWithoutDisclosingSecret()
    {
        const string invalid = "invalid-certificate-fixture";
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["AiData:KeyEncryptionCertificateBase64"] = invalid }).Build();
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddAiCredentialProtection(config));
        Assert.DoesNotContain(invalid, error.Message);
    }

    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MigrationLegacyImportAndNewInstancesKeepApiKeyAcrossRedeploy(bool useCertificate)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "ai_keys_" + Guid.NewGuid().ToString("N");
        var tempRoot = Path.GetFullPath(Path.GetTempPath());
        var legacyDirectory = Path.Combine(tempRoot, "cdsqg-ai-keys-" + Guid.NewGuid().ToString("N"));
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=CDSQG test key wrapping", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["AiData:ProtectionKeyDirectory"] = legacyDirectory,
            ["AiData:KeyEncryptionCertificateBase64"] = useCertificate ? Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, "fixture")) : null,
            ["AiData:KeyEncryptionCertificatePassword"] = useCertificate ? "fixture" : null
        }).Build();
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        var scopedConnection = new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString;
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(scopedConnection).Options;
        ServiceProvider NewInstance()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(scopedConnection));
            services.AddAiCredentialProtection(configuration);
            services.Configure<AiDataOptions>(_ => { });
            services.AddScoped<AiSettingsStore>();
            return services.BuildServiceProvider();
        }
        void DeleteOwnTestDirectory()
        {
            var resolved = Path.GetFullPath(legacyDirectory);
            Assert.StartsWith(tempRoot, resolved, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith("cdsqg-ai-keys-", Path.GetFileName(resolved));
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        try
        {
            await using var db = new AppDbContext(options);
            await db.GetService<IMigrator>().MigrateAsync("20261022140000_AddGoalResultTarget");
            Directory.CreateDirectory(legacyDirectory);
            var legacyServices = new ServiceCollection();
            legacyServices.AddLogging();
            legacyServices.AddDataProtection().SetApplicationName(AiCredentialProtection.ApplicationName).PersistKeysToFileSystem(new DirectoryInfo(legacyDirectory));
            using (var legacy = legacyServices.BuildServiceProvider())
                db.AiProviderConfigurations.Add(new AiProviderConfiguration { Id = 1, Enabled = true, BaseUrl = "https://example.com/v1", Model = "fixture-model", ProtectedApiKey = legacy.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose).Protect("fixture-legacy") });
            await db.SaveChangesAsync();
            var originalCiphertext = (await db.AiProviderConfigurations.SingleAsync()).ProtectedApiKey;
            await db.Database.MigrateAsync();
            Assert.Equal(originalCiphertext, (await db.AiProviderConfigurations.AsNoTracking().SingleAsync()).ProtectedApiKey);
            await using (var concurrentDb = new AppDbContext(options))
            {
                var imports = await Task.WhenAll(AiCredentialProtection.ImportLegacyKeysAsync(db, configuration, "unused"), AiCredentialProtection.ImportLegacyKeysAsync(concurrentDb, configuration, "unused"));
                Assert.Equal(1, imports.Sum());
            }
            Assert.Equal(0, await AiCredentialProtection.ImportLegacyKeysAsync(db, configuration, "unused"));
            Assert.Equal(1, await db.DataProtectionKeys.CountAsync());
            using (var first = NewInstance())
            {
                using var scope = first.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<AiSettingsStore>();
                Assert.Equal("fixture-legacy", (await store.GetAsync()).ApiKey);
                first.GetRequiredService<IKeyManager>().CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(90));
                await store.SaveAsync(Input("fixture-updated"), "fixture", default);
            }
            DeleteOwnTestDirectory(); // A new deploy has no access to the previous container's files.
            using (var restarted = NewInstance())
            using (var scope = restarted.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<AiSettingsStore>();
                Assert.Equal("fixture-updated", (await store.GetAsync()).ApiKey);
                await store.SaveAsync(Input(), "fixture", default);
                Assert.DoesNotContain("fixture-updated", JsonSerializer.Serialize(await store.PublicAsync(default)));
            }
            using (var anotherInstance = NewInstance())
            using (var scope = anotherInstance.CreateScope())
                Assert.Equal("fixture-updated", (await scope.ServiceProvider.GetRequiredService<AiSettingsStore>().GetAsync()).ApiKey);
            var keys = await db.DataProtectionKeys.AsNoTracking().ToListAsync();
            Assert.True(keys.Count >= 2);
            Assert.DoesNotContain(keys, key => key.Xml!.Contains("fixture-updated"));
            if (useCertificate) Assert.Contains(keys, key => key.Xml!.Contains("EncryptedData"));
            await db.Database.MigrateAsync();
            Assert.Equal(keys.Count, await db.DataProtectionKeys.CountAsync());
        }
        finally
        {
            DeleteOwnTestDirectory();
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
}
