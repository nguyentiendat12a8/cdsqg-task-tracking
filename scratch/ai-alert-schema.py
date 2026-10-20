from pathlib import Path
p=Path('backend/Infrastructure/Data/AppDbContext.cs');s=p.read_text(encoding='utf-8-sig').replace('        public DbSet<AiInboxItem>', '        public DbSet<AiAlertFeed> AiAlertFeeds => Set<AiAlertFeed>();\n        public DbSet<AiAlertScan> AiAlertScans => Set<AiAlertScan>();\n        public DbSet<AiInboxItem>');needle='            modelBuilder.Entity<AiInboxItem>().HasIndex';at=s.index(needle);s=s[:at]+'''            modelBuilder.Entity<AiAlertFeed>().HasIndex(x=>x.FeedUrl).IsUnique();
            modelBuilder.Entity<AiAlertScan>().HasOne<AiAlertFeed>().WithMany().HasForeignKey(x=>x.FeedId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AiAlertScan>().HasIndex(x=>new{x.Status,x.LeaseUntil});
            modelBuilder.Entity<AiAlertScan>().HasIndex(x=>x.FeedId).IsUnique().HasFilter("\\\"Status\\\" IN ('Queued', 'Running')");
            modelBuilder.Entity<AiInboxItem>().HasOne<AiAlertFeed>().WithMany().HasForeignKey(x=>x.AlertFeedId).OnDelete(DeleteBehavior.Restrict);
'''+s[at:];p.write_text(s,encoding='utf-8')
p=Path('backend/Core/Entities/AiData.cs');s=p.read_text(encoding='utf-8-sig').replace('public class AiInboxItem\n{','public class AiInboxItem\n{\n    public Guid? AlertFeedId {get;set;}\n    public DateTime? PublishedAt {get;set;}');p.write_text(s,encoding='utf-8')
