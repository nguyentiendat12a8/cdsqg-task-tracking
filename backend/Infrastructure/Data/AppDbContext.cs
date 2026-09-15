using Microsoft.EntityFrameworkCore;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using System.Text.Json;
using System.Collections.Generic;

namespace Cdsqg.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agency> Agencies => Set<Agency>();
        public DbSet<UnitDictionary> Units => Set<UnitDictionary>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<GoalTaskItem> GoalTaskItems => Set<GoalTaskItem>();
        public DbSet<TargetBaseline> TargetBaselines => Set<TargetBaseline>();
        public DbSet<ProgressLog> ProgressLogs => Set<ProgressLog>();
        public DbSet<TaskUrgeLog> TaskUrgeLogs => Set<TaskUrgeLog>();
        public DbSet<DataImportLog> DataImportLogs => Set<DataImportLog>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------
            // USER ACCOUNT CONFIGURATION
            // ----------------------------------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();

                entity.HasOne(e => e.Agency)
                      .WithMany()
                      .HasForeignKey(e => e.AgencyId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ----------------------------------------------------
            // MODULE 1: AGENCY & UNIT DICTIONARY CATALOGS
            // ----------------------------------------------------
            modelBuilder.Entity<Agency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
                entity.Property(e => e.Type).HasConversion<string>();
            });

            modelBuilder.Entity<UnitDictionary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.DataType).HasConversion<string>();
            });

            // ----------------------------------------------------
            // MODULE 2: DOCUMENT & TASK HIERARCHY
            // ----------------------------------------------------
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DocumentNumber).IsUnique();
                entity.Property(e => e.TimeResolution).HasConversion<string>();

                entity.Property(e => e.AttachmentFilePaths)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      );
            });

            // ----------------------------------------------------
            // MODULE 3: GOAL/TASK ITEM WITH JSONB CUSTOM BASELINE
            // ----------------------------------------------------
            modelBuilder.Entity<GoalTaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemType).HasConversion<string>();
                entity.Property(e => e.EvaluationType).HasConversion<string>();
                entity.Property(e => e.CalculationMethod).HasConversion<string>();

                // Lead Agency Relationship
                entity.HasOne(e => e.LeadAgency)
                      .WithMany()
                      .HasForeignKey(e => e.LeadAgencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unit Dictionary Relationship
                entity.HasOne(e => e.Unit)
                      .WithMany()
                      .HasForeignKey(e => e.UnitId)
                      .OnDelete(DeleteBehavior.SetNull);

                // -----------------------------------------------------------------------
                // CRUCIAL BRD REQUIREMENT: POSTGRESQL JSONB CONFIGURATION FOR CustomBaseline
                // Flexible storage for manual quarterly/monthly overrides (e.g. {"Q1_2026": 10.0, "Q2_2026": 40.0})
                // -----------------------------------------------------------------------
                entity.Property(e => e.CustomBaseline)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                      );

                // PostgreSQL JSONB mapping for CoordinatingAgencyIds array of UUIDs
                entity.Property(e => e.CoordinatingAgencyIds)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<System.Guid>>(v, (JsonSerializerOptions?)null) ?? new List<System.Guid>()
                      );

                // PostgreSQL JSONB mapping for DynamicKPIs flexible attributes
                entity.Property(e => e.DynamicKPIs)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                      );
            });

            // ----------------------------------------------------
            // TARGET BASELINE CONFIGURATION
            // ----------------------------------------------------
            modelBuilder.Entity<TargetBaseline>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.GoalTaskId, e.Year, e.Quarter }).IsUnique();
                entity.Property(e => e.TargetQualitativeStatus).HasConversion<string>();

                entity.HasOne(e => e.GoalTaskItem)
                      .WithMany(g => g.Baselines)
                      .HasForeignKey(e => e.GoalTaskId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----------------------------------------------------
            // MODULE 4 & 5: PROGRESS LOG WITH JSONB ATTACHMENT FILES
            // ----------------------------------------------------
            modelBuilder.Entity<ProgressLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QualitativeStatus).HasConversion<string>();
                entity.Property(e => e.CalculatedAlert).HasConversion<string>();

                // PostgreSQL JSONB mapping for AttachmentFileUrls list
                entity.Property(e => e.AttachmentFileUrls)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      );

                entity.HasOne(e => e.GoalTaskItem)
                      .WithMany(g => g.ProgressLogs)
                      .HasForeignKey(e => e.GoalTaskId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
