using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using Cdsqg.Core.Entities;

using Cdsqg.Core.Enums;

using System.Text.Json;

using System.Collections.Generic;



namespace Cdsqg.Infrastructure.Data

{

    public class AppDbContext : DbContext, IDataProtectionKeyContext

    {

        // Legacy imports stored camelCase while EF writes PascalCase. Preserve persisted

        // product IDs in either format instead of running TaskDeliverable's ID initializer.

        private static readonly JsonSerializerOptions DeliverableReadOptions = new() { PropertyNameCaseInsensitive = true };



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

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<AgencyTaskExecution> AgencyTaskExecutions => Set<AgencyTaskExecution>();

        public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();



        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        public DbSet<AiProviderConfiguration> AiProviderConfigurations => Set<AiProviderConfiguration>();

        public DbSet<AiAlertFeed> AiAlertFeeds => Set<AiAlertFeed>();

        public DbSet<AiAlertScan> AiAlertScans => Set<AiAlertScan>();

        public DbSet<AiInboxItem> AiInboxItems => Set<AiInboxItem>();

        public DbSet<AiGoalDataLink> AiGoalDataLinks => Set<AiGoalDataLink>();

        public DbSet<AiDataSource> AiDataSources => Set<AiDataSource>();

        public DbSet<AiSourceDocument> AiSourceDocuments => Set<AiSourceDocument>();

        public DbSet<AiDataSuggestion> AiDataSuggestions => Set<AiDataSuggestion>();

        public DbSet<AiGoalKeyword> AiGoalKeywords => Set<AiGoalKeyword>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AiAlertFeed>().HasIndex(x=>new{x.DocumentId,x.FeedUrl}).IsUnique();

            modelBuilder.Entity<AiAlertScan>().HasOne<AiAlertFeed>().WithMany().HasForeignKey(x=>x.FeedId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiAlertScan>().HasIndex(x=>new{x.Status,x.LeaseUntil});

            modelBuilder.Entity<AiAlertScan>().HasIndex(x=>x.FeedId).IsUnique().HasFilter("\"Status\" IN ('Queued', 'Running')");

            modelBuilder.Entity<AiInboxItem>().HasOne<AiAlertFeed>().WithMany().HasForeignKey(x=>x.AlertFeedId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiInboxItem>().HasIndex(x=>x.UrlHash).IsUnique();

            modelBuilder.Entity<AiSourceDocument>().HasOne<AiInboxItem>().WithMany().HasForeignKey(x=>x.InboxItemId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.HasSequence<long>("AiInboxNumbers");

            modelBuilder.Entity<AiInboxQueueState>().HasData(new AiInboxQueueState { Id=1 });

            modelBuilder.Entity<AiInboxItem>().Property(x=>x.SequenceNumber).HasDefaultValueSql("nextval('\"AiInboxNumbers\"')");

            modelBuilder.Entity<AiInboxItem>().HasIndex(x=>x.SequenceNumber).IsUnique();

            modelBuilder.Entity<AiInboxItem>().HasIndex(x=>new{x.Status,x.LeaseUntil});

            modelBuilder.Entity<AiGoalDataLink>().HasKey(x => new { x.GoalTaskId, x.SuggestionId });

            modelBuilder.Entity<AiGoalDataLink>().HasOne<ProgressLog>().WithMany().HasForeignKey(x => x.ProgressLogId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiGoalDataLink>().HasOne<GoalTaskItem>().WithMany().HasForeignKey(x => x.GoalTaskId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiGoalDataLink>().HasOne<AiDataSuggestion>().WithMany().HasForeignKey(x => x.SuggestionId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiDataSource>().HasData(

                new AiDataSource { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), Name = "UN E-Government Survey (trang báo cáo mẫu 2024)", MetricKey = "EGDI_RANK", Url = "https://desapublications.un.org/publications/un-e-government-survey-2024", IntervalHours = 168 },

                new AiDataSource { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), Name = "Cục Thống kê - Tài khoản quốc gia", MetricKey = "DIGITAL_GDP_SHARE", Url = "https://www.nso.gov.vn/tai-khoan-quoc-gia/", IntervalHours = 168 },

                new AiDataSource { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), Name = "Sách trắng TMĐT (PDF mẫu 2022, cần chọn kỳ mới)", MetricKey = "ECOM_RETAIL_SHARE", Url = "https://moit.gov.vn/upload/2005517/fck/files/BCTMDT2022-final-pdf_08c88.PDF", IntervalHours = 168 }

            );

            modelBuilder.Entity<AiDataSource>().HasIndex(x => new { x.Enabled, x.NextCheckAt });

            modelBuilder.Entity<AiDataSource>().Property(x => x.Name).HasMaxLength(250);

            modelBuilder.Entity<AiDataSource>().Property(x => x.MetricKey).HasMaxLength(50);

            modelBuilder.Entity<AiSourceDocument>().HasIndex(x => new { x.SourceId, x.Url, x.ContentHash });

            modelBuilder.Entity<AiSourceDocument>().HasOne<AiDataSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiDataSuggestion>().HasOne<AiSourceDocument>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiDataSuggestion>().HasIndex(x => new { x.DocumentId, x.MetricKey, x.DataYear, x.Value, x.Page }).IsUnique().HasFilter("\"TargetAgencyId\" IS NULL");

            modelBuilder.Entity<AiDataSuggestion>().HasIndex(x => new { x.DocumentId, x.MetricKey, x.DataYear, x.Value, x.Page, x.TargetAgencyId }).IsUnique().HasFilter("\"TargetAgencyId\" IS NOT NULL");

            modelBuilder.Entity<AiAlertFeed>().Property(x=>x.DocumentId).HasDefaultValue(Guid.Parse("12660000-0000-0000-0000-000000001266"));
            modelBuilder.Entity<AiAlertFeed>().Property(x=>x.MetricKeysJson).HasDefaultValue("[]");

            modelBuilder.Entity<AiAlertFeed>().Property(x=>x.AgencyIdsJson).HasDefaultValue("[]");

            modelBuilder.Entity<AiDataSuggestion>().HasIndex(x => new { x.MetricKey, x.DataYear }).IsUnique().HasFilter("\"IsCurrent\" = TRUE AND \"TargetAgencyId\" IS NULL");

            modelBuilder.Entity<AiDataSuggestion>().HasIndex(x => new { x.MetricKey, x.DataYear, x.TargetAgencyId }).IsUnique().HasFilter("\"IsCurrent\" = TRUE AND \"TargetAgencyId\" IS NOT NULL");

            modelBuilder.Entity<AiDataSuggestion>().HasOne<Agency>().WithMany().HasForeignKey(x => x.TargetAgencyId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiDataSuggestion>().Property(x => x.Value).HasPrecision(18, 6);

            modelBuilder.Entity<AiDataSuggestion>().Property(x => x.AcceptedValue).HasPrecision(18, 6);

            modelBuilder.Entity<AiGoalKeyword>().HasIndex(x => new { x.DocumentId, x.GoalCode }).IsUnique();

            modelBuilder.Entity<AiGoalKeyword>().Property(x => x.GoalCode).HasMaxLength(20);

            modelBuilder.Entity<AiGoalKeyword>().Property(x => x.ExpectedUnit).HasMaxLength(50);

            modelBuilder.Entity<AiGoalKeyword>().HasData(

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000001"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-01",

                    GoalTitle = "100% hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa",

                    KeywordsVi = "số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử, bưu chính công ích",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000002"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-02",

                    GoalTitle = "99% người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến",

                    KeywordsVi = "đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000003"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-03",

                    GoalTitle = "100% bộ, ngành, địa phương sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công",

                    KeywordsVi = "nền tảng số dùng chung, bộ ngành địa phương, chỉ đạo điều hành, dịch vụ công, trục liên thông văn bản, ndxp",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ bộ, ngành, địa phương sử dụng nền tảng số dùng chung quốc gia phục vụ chỉ đạo điều hành và dịch vụ công (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000004"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-04",

                    GoalTitle = "100% hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu",

                    KeywordsVi = "kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, tích hợp dữ liệu",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hệ thống thông tin có kết nối, chia sẻ dữ liệu qua nền tảng tích hợp, chia sẻ dữ liệu (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000005"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-05",

                    GoalTitle = "100% cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI",

                    KeywordsVi = "cơ sở dữ liệu dùng chung, csdl quốc gia, csdl chuyên ngành, chuẩn hóa dữ liệu, sẵn sàng khai thác ai, trí tuệ nhân tạo",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cơ sở dữ liệu dùng chung quốc gia, ngành, địa phương được chuẩn hóa và sẵn sàng kết nối, khai thác AI (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000006"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-06",

                    GoalTitle = "100% trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định",

                    KeywordsVi = "trung tâm điều hành thông minh, ioc, cấp tỉnh, tích hợp ai, dự báo cảnh báo, giám sát ra quyết định",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ trung tâm điều hành thông minh (IOC) cấp tỉnh được nâng cấp tích hợp AI (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000007"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-07",

                    GoalTitle = "100% cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định",

                    KeywordsVi = "cán bộ công chức viên chức, hồ sơ điện tử, quản lý cán bộ, cơ sở dữ liệu cán bộ công chức, cbccvc, bộ nội vụ",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000008"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-08",

                    GoalTitle = "70% số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính",

                    KeywordsVi = "chỉ tiêu thống kê, dữ liệu hành chính, hệ thống chỉ tiêu, tổng cục thống kê, cục thống kê, số liệu hành chính",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000009"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-09",

                    GoalTitle = "100% cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số",

                    KeywordsVi = "đào tạo kỹ năng số, bồi dưỡng kiến thức số, cán bộ công chức, chuyển đổi số công vụ, tập huấn kỹ năng số",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng cập nhật kỹ năng số (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000010"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-10",

                    GoalTitle = "100% cán bộ chuyên trách về chuyển đổi số, công nghệ thông tin được đào tạo, bồi dưỡng, cập nhật công nghệ số",

                    KeywordsVi = "chuyên trách cntt, chuyên trách chuyển đổi số, bồi dưỡng công nghệ số, cán bộ công nghệ thông tin",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ chuyên trách chuyển đổi số, CNTT được đào tạo, bồi dưỡng nâng cao công nghệ số (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000011"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-11",

                    GoalTitle = "100% cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ",

                    KeywordsVi = "trợ lý ai công vụ, trợ lý ảo công vụ, tài khoản trợ lý ai, đào tạo trợ lý ảo, ai phục vụ cán bộ",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ, công chức, viên chức có tài khoản và được tập huấn sử dụng trợ lý AI công vụ (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000012"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-12",

                    GoalTitle = "Việt Nam thuộc nhóm 50 nước dẫn đầu trên thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",

                    KeywordsVi = "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",

                    KeywordsEn = "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",

                    ExpectedUnit = "rank",

                    ExtractionPrompt = "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000013"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-13",

                    GoalTitle = "Quy mô kinh tế số đạt tối thiểu 30% GDP",

                    KeywordsVi = "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000014"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-14",

                    GoalTitle = "Hỗ trợ tối thiểu 500.000 doanh nghiệp nhỏ và vừa chuyển đổi số",

                    KeywordsVi = "doanh nghiệp nhỏ và vừa, sme, chuyển đổi số doanh nghiệp, smedx, hỗ trợ doanh nghiệp",

                    KeywordsEn = "",

                    ExpectedUnit = "doanh nghiệp",

                    ExtractionPrompt = "Xác định số lượng doanh nghiệp nhỏ và vừa (SME) được hỗ trợ chuyển đổi số (đơn vị: doanh nghiệp).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000015"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-15",

                    GoalTitle = "Phát triển, thúc đẩy và đưa vào sử dụng tối thiểu 5 sàn dữ liệu, bảo đảm an toàn, tuân thủ quy định của pháp luật",

                    KeywordsVi = "sàn dữ liệu, trung tâm giao dịch dữ liệu, thị trường dữ liệu, data exchange, sàn giao dịch dữ liệu",

                    KeywordsEn = "",

                    ExpectedUnit = "sàn",

                    ExtractionPrompt = "Xác định số lượng sàn dữ liệu / trung tâm giao dịch dữ liệu được đưa vào vận hành (đơn vị: sàn).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000016"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-16",

                    GoalTitle = "Tỷ trọng doanh thu thương mại điện tử so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng đạt từ 15 - 17%",

                    KeywordsVi = "thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000017"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-17",

                    GoalTitle = "Phấn đấu đạt 45% số doanh nghiệp nhỏ và vừa hoạt động trong lĩnh vực thương mại tham gia các sàn thương mại điện tử lớn trong và ngoài nước",

                    KeywordsVi = "sme thương mại, sàn thương mại điện tử, tham gia sàn tmđt, shopee, lazada, tiki, alibaba, amazon, bán hàng trực tuyến",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ doanh nghiệp nhỏ và vừa ngành thương mại tham gia các sàn thương mại điện tử (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000018"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-18",

                    GoalTitle = "Giá trị thanh toán không dùng tiền mặt gấp 30 lần GDP",

                    KeywordsVi = "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp, giao dịch phi tiền mặt",

                    KeywordsEn = "",

                    ExpectedUnit = "lần GDP",

                    ExtractionPrompt = "Xác định tỷ lệ giá trị thanh toán không dùng tiền mặt so với GDP (số lần GDP).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000019"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-19",

                    GoalTitle = "Trên 40% doanh nghiệp nhỏ và vừa tiếp cận, sử dụng nền tảng “mô hình như một dịch vụ”, tác nhân AI hoặc giải pháp AI dùng chung",

                    KeywordsVi = "mô hình như một dịch vụ, maas, tác nhân ai, ai agent, giải pháp ai dùng chung, sme ứng dụng ai",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ doanh nghiệp nhỏ và vừa tiếp cận hoặc sử dụng giải pháp AI / mô hình như một dịch vụ (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000020"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-20",

                    GoalTitle = "Hạ tầng mạng băng rộng cáp quang phủ 100% hộ gia đình",

                    KeywordsVi = "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet, phủ cáp quang",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hộ gia đình có kết nối mạng băng rộng cáp quang (FTTH) (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000021"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-21",

                    GoalTitle = "Tỷ lệ dân số từ 15 tuổi trở lên có tài khoản giao dịch tại ngân hàng hoặc tổ chức được phép khác đạt tối thiểu 95%",

                    KeywordsVi = "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng, tài khoản thanh toán",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ dân số từ 15 tuổi trở lên có tài khoản thanh toán / giao dịch tại ngân hàng (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000022"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-22",

                    GoalTitle = "Tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân đạt trên 70%",

                    KeywordsVi = "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số cá nhân, vnpt smartca, viettel ca",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000023"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-23",

                    GoalTitle = "Phấn đấu tỷ lệ người dân được lập sổ sức khỏe điện tử đạt 100%",

                    KeywordsVi = "sổ sức khỏe điện tử, hồ sơ sức khỏe điện tử, y tế số, vneid sức khỏe, bộ y tế, bảo hiểm y tế",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ người dân được lập sổ / hồ sơ sức khỏe điện tử (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000024"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-24",

                    GoalTitle = "Đào tạo, bồi dưỡng, tập huấn kỹ năng số cơ bản cho tối thiểu 10.000.000 lượt người trong độ tuổi lao động",

                    KeywordsVi = "kỹ năng số cơ bản, độ tuổi lao động, tập huấn kỹ năng số, phổ cập kỹ năng số, lượt người",

                    KeywordsEn = "",

                    ExpectedUnit = "lượt người",

                    ExtractionPrompt = "Xác định số lượt người trong độ tuổi lao động được đào tạo, bồi dưỡng kỹ năng số cơ bản (đơn vị: lượt người).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000025"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-25",

                    GoalTitle = "Việt Nam thuộc nhóm 30 nước dẫn đầu thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",

                    KeywordsVi = "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",

                    KeywordsEn = "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",

                    ExpectedUnit = "rank",

                    ExtractionPrompt = "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000026"),

                    DocumentId = Guid.Parse("12660000-0000-0000-0000-000000001266"),

                    GoalCode = "MT-26",

                    GoalTitle = "Quy mô kinh tế số đạt tối thiểu 50% GDP, kinh tế số trở thành một trong những động lực phát triển chủ yếu của nền kinh tế",

                    KeywordsVi = "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, 50% gdp, động lực phát triển",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000001"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-01",

                    GoalTitle = "Tỉ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện.",

                    KeywordsVi = "dịch vụ công trực tuyến toàn trình, dvctt toàn trình, thủ tục hành chính đủ điều kiện, tthc đủ điều kiện, dịch vụ công toàn trình, cung cấp trực tuyến toàn trình",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện (%). Chỉ lấy số liệu thực tế đã công bố, không lấy ngưỡng mục tiêu.",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000002"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-02",

                    GoalTitle = "Tỉ lệ thông tin, giấy tờ, tài liệu trong các thủ tục hành chính liên quan đến hoạt động sản xuất, kinh doanh chỉ cung cấp một lần cho cơ quan hành chính nhà nước.",

                    KeywordsVi = "chỉ cung cấp một lần, sản xuất kinh doanh, thủ tục hành chính doanh nghiệp, giấy tờ tài liệu một lần, chia sẻ thông tin thủ tục, cung cấp một lần",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ thông tin, giấy tờ trong TTHC về sản xuất kinh doanh chỉ phải cung cấp một lần cho cơ quan nhà nước (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000003"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-03",

                    GoalTitle = "Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.",

                    KeywordsVi = "đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000004"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-04",

                    GoalTitle = "Tỉ lệ các nền tảng số quốc gia, nền tảng số dùng chung của ngành, lĩnh vực phục vụ Chính phủ số được hoàn thành và đưa vào sử dụng thống nhất từ Trung ương đến địa phương theo đúng kế hoạch.",

                    KeywordsVi = "nền tảng số quốc gia, nền tảng số dùng chung, chính phủ số, hoàn thành đưa vào sử dụng, sử dụng thống nhất, nền tảng ngành lĩnh vực",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ nền tảng số quốc gia, nền tảng dùng chung phục vụ Chính phủ số hoàn thành và đưa vào sử dụng thống nhất (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000005"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-05",

                    GoalTitle = "Tỉ lệ hồ sơ công việc tại cấp bộ, tỉnh, cấp xã được xử lý trên môi trường điện tử (trừ hồ sơ công việc thuộc phạm vi bí mật nhà nước) và sử dụng chữ ký số cá nhân để giải quyết công việc.",

                    KeywordsVi = "hồ sơ công việc, môi trường điện tử, cấp bộ, cấp tỉnh, cấp xã, chữ ký số cá nhân, xử lý văn bản điện tử, hệ thống quản lý văn bản",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hồ sơ công việc tại cấp bộ, tỉnh, xã được xử lý trên môi trường điện tử và có dùng chữ ký số cá nhân (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000006"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-06",

                    GoalTitle = "Tỉ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo.",

                    KeywordsVi = "chế độ báo cáo, hệ thống thông tin báo cáo, hệ thống báo cáo quốc gia, báo cáo điện tử, số hóa chế độ báo cáo",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000007"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-07",

                    GoalTitle = "Tỉ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao cho các bộ, ngành, địa phương được theo dõi, giám sát, đánh giá trên môi trường điện tử.",

                    KeywordsVi = "nhiệm vụ chính phủ giao, thủ tướng chính phủ giao, theo dõi giám sát đánh giá, môi trường điện tử, hệ thống theo dõi nhiệm vụ",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao được theo dõi, giám sát trên môi trường điện tử (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000008"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-08",

                    GoalTitle = "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.",

                    KeywordsVi = "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 3, mức độ trưởng thành dữ liệu, đánh giá quản trị dữ liệu",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 3 về mức độ trưởng thành quản trị dữ liệu (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000009"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-09",

                    GoalTitle = "Tỉ lệ cán bộ, công chức, viên chức được đào tạo kỹ năng số cơ bản.",

                    KeywordsVi = "đào tạo kỹ năng số, kỹ năng số cơ bản, cán bộ công chức viên chức, bồi dưỡng kỹ năng số, phổ cập kỹ năng số công chức",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng kỹ năng số cơ bản (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000010"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-10",

                    GoalTitle = "Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.",

                    KeywordsVi = "chuyên trách cntt, chuyên trách chuyển đổi số, chứng chỉ quản trị dữ liệu, đào tạo quản trị dữ liệu cơ bản",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000011"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-11",

                    GoalTitle = "Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.",

                    KeywordsVi = "đánh giá hài lòng dvctt, mức độ hài lòng người dân, cổng dịch vụ công, sự hài lòng dịch vụ công trực tuyến",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (giai đoạn 2028-2030) (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000012"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-12",

                    GoalTitle = "Tỉ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, được AI hỗ trợ “cá thể hóa”, thông báo hoặc gợi ý hành động theo các mốc sự kiện quan trọng trong cuộc đời.",

                    KeywordsVi = "dịch vụ công thiết yếu, dịch vụ công chủ động, ai cá thể hóa, gợi ý hành động, mốc sự kiện cuộc đời, trợ lý ai dịch vụ công",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, có AI hỗ trợ cá thể hóa theo mốc sự kiện (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000013"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-13",

                    GoalTitle = "Tỉ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng chuẩn.",

                    KeywordsVi = "dữ liệu mở, open data, dữ liệu mở đúng chuẩn, cổng dữ liệu quốc gia, data.gov.vn, cung cấp dữ liệu mở",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng tiêu chuẩn, định dạng mở (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000014"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-14",

                    GoalTitle = "Tỉ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa.",

                    KeywordsVi = "số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000015"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-15",

                    GoalTitle = "Tỉ lệ khai thác, sử dụng lại thông tin, dữ liệu đã được số hóa trong giải quyết thủ tục hành chính, dịch vụ công.",

                    KeywordsVi = "khai thác dữ liệu số hóa, sử dụng lại thông tin, tái sử dụng dữ liệu, số hóa trong tthc, không yêu cầu nộp lại giấy tờ",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ khai thác, tái sử dụng thông tin, dữ liệu đã số hóa trong giải quyết TTHC, dịch vụ công (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000016"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-16",

                    GoalTitle = "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu.",

                    KeywordsVi = "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 4, mức độ trưởng thành dữ liệu cấp 4",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000017"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-17",

                    GoalTitle = "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu.",

                    KeywordsVi = "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 5, mức độ trưởng thành dữ liệu tối ưu cấp 5",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000018"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-18",

                    GoalTitle = "Tỉ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu.",

                    KeywordsVi = "kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, dịch vụ chia sẻ dữ liệu",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000019"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-19",

                    GoalTitle = "Tỉ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI để phục vụ công tác chỉ đạo, điều hành.",

                    KeywordsVi = "ứng dụng ai, trí tuệ nhân tạo, chỉ đạo điều hành, trợ lý ảo, ai phục vụ điều hành, cơ quan tổ chức sử dụng ai",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI phục vụ công tác chỉ đạo, điều hành (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000020"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-20",

                    GoalTitle = "Tỉ lệ đội ngũ lãnh đạo của từng cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học, kỹ thuật, chuyển đổi số phù hợp với yêu cầu nhiệm vụ.",

                    KeywordsVi = "đội ngũ lãnh đạo, lãnh đạo chuyển đổi số, chuyên môn kỹ thuật, kinh nghiệm khoa học kỹ thuật chuyển đổi số, cán bộ lãnh đạo",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ đội ngũ lãnh đạo của cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học kỹ thuật, chuyển đổi số (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000021"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-21",

                    GoalTitle = "Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.",

                    KeywordsVi = "chuyên trách cntt chuyển đổi số, chứng chỉ quản trị dữ liệu cơ bản, đào tạo chuyên trách quản trị dữ liệu",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (mục tiêu 2028-2030) (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                },

                new AiGoalKeyword

                {

                    Id = Guid.Parse("b2000000-0000-0000-0000-000000000022"),

                    DocumentId = Guid.Parse("26290000-0000-0000-0000-000000002629"),

                    GoalCode = "MT-22",

                    GoalTitle = "Tỉ lệ các hệ thống thông tin phục vụ phát triển Chính phủ số phải được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ trước khi đưa vào vận hành, sử dụng; được kiểm tra, đánh giá an ninh mạng định kỳ.",

                    KeywordsVi = "an ninh mạng theo cấp độ, an toàn thông tin theo cấp độ, phê duyệt hồ sơ cấp độ, kiểm tra an ninh mạng định kỳ, bảo đảm an toàn thông tin",

                    KeywordsEn = "",

                    ExpectedUnit = "%",

                    ExtractionPrompt = "Xác định tỷ lệ hệ thống thông tin Chính phủ số được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ và kiểm tra định kỳ (%).",

                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

                }

            );





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

            // NOTIFICATION CONFIGURATION

            // ----------------------------------------------------

            modelBuilder.Entity<Notification>(entity =>

            {

                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)

                      .WithMany()

                      .HasForeignKey(e => e.UserId)

                      .OnDelete(DeleteBehavior.Cascade);



                entity.HasOne(e => e.Agency)

                      .WithMany()

                      .HasForeignKey(e => e.AgencyId)

                      .OnDelete(DeleteBehavior.Cascade);

            });



            // ----------------------------------------------------

            // MODULE 1: AGENCY (WITH SUB-AGENCIES HIERARCHY)

            // ----------------------------------------------------

            modelBuilder.Entity<Agency>(entity =>

            {

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(250);

                entity.Property(e => e.Type).HasConversion<string>();



                entity.Property(e => e.ContactPersons)

                      .HasConversion(

                          v => JsonSerializer.Serialize(v ?? new List<AgencyContactPerson>(), (JsonSerializerOptions?)null),

                          v => string.IsNullOrWhiteSpace(v)

                              ? new List<AgencyContactPerson>()

                              : JsonSerializer.Deserialize<List<AgencyContactPerson>>(v, (JsonSerializerOptions?)null) ?? new List<AgencyContactPerson>()

                      );



                entity.Property(e => e.PlanFiles)

                      .HasConversion(

                          v => JsonSerializer.Serialize(v ?? new List<AgencyPlanFile>(), (JsonSerializerOptions?)null),

                          v => string.IsNullOrWhiteSpace(v)

                              ? new List<AgencyPlanFile>()

                              : JsonSerializer.Deserialize<List<AgencyPlanFile>>(v, (JsonSerializerOptions?)null) ?? new List<AgencyPlanFile>()

                      );



                entity.HasOne(e => e.ParentAgency)

                      .WithMany(a => a.ChildAgencies)

                      .HasForeignKey(e => e.ParentId)

                      .OnDelete(DeleteBehavior.Restrict);

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

            // MODULE 3: GOAL/TASK ITEM WITH SUB-TASKS HIERARCHY

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



                entity.Property(e => e.CustomBaseline)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()

                      );



                entity.Property(e => e.CoordinatingAgencyIds)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<System.Guid>>(v, (JsonSerializerOptions?)null) ?? new List<System.Guid>()

                      );



                entity.Property(e => e.DynamicKPIs)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()

                      );



                entity.Property(e => e.Deliverables)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<TaskDeliverable>>(v, DeliverableReadOptions) ?? new List<TaskDeliverable>()

                      );



                entity.Property(e => e.AgencyDeliverables)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<Dictionary<string, List<TaskDeliverable>>>(v, DeliverableReadOptions) ?? new Dictionary<string, List<TaskDeliverable>>()

                      );

            });



            // ----------------------------------------------------

            // TARGET BASELINE CONFIGURATION

            // ----------------------------------------------------

            modelBuilder.Entity<TargetBaseline>(entity =>

            {

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.GoalTaskId, e.Year }).IsUnique();

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



                entity.Property(e => e.AttachmentFileUrls)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()

                      );



                entity.Property(e => e.Deliverables)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<TaskDeliverable>>(v, DeliverableReadOptions) ?? new List<TaskDeliverable>()

                      );



                entity.HasOne(e => e.GoalTaskItem)

                      .WithMany(g => g.ProgressLogs)

                      .HasForeignKey(e => e.GoalTaskId)

                      .OnDelete(DeleteBehavior.Cascade);

            });



            // ----------------------------------------------------

            // AGENCY TASK EXECUTION CONFIGURATION

            // ----------------------------------------------------

            modelBuilder.Entity<AgencyTaskExecution>(entity =>

            {

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.GoalTaskId, e.AgencyId }).IsUnique();

                entity.Property(e => e.AssignedAt).HasColumnType("timestamp without time zone");

                entity.HasIndex(e => new { e.AgencyId, e.IsLeadAssignment, e.GoalTaskId }).HasDatabaseName("IX_Execution_Agency_Lead_Task");

                entity.HasIndex(e => new { e.AgencyId, e.IsCoordinatingAssignment, e.GoalTaskId }).HasDatabaseName("IX_Execution_Agency_Coordinating_Task");

                entity.Property(e => e.CalculatedStatus).HasConversion<string>();

                entity.Property(e => e.LatestQualitativeStatus).HasConversion<string>();



                entity.Property(e => e.Deliverables)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<TaskDeliverable>>(v, DeliverableReadOptions) ?? new List<TaskDeliverable>()

                      );



                entity.Property(e => e.AttachmentFileUrls)

                      .HasColumnType("jsonb")

                      .HasConversion(

                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),

                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()

                      );



                entity.HasOne(e => e.GoalTaskItem)

                      .WithMany(g => g.AgencyExecutions)

                      .HasForeignKey(e => e.GoalTaskId)

                      .OnDelete(DeleteBehavior.Cascade);



                entity.HasOne(e => e.Agency)

                      .WithMany()

                      .HasForeignKey(e => e.AgencyId)

                      .OnDelete(DeleteBehavior.Cascade);

            });

        }

    }

}

