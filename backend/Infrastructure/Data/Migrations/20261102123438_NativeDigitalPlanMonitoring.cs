using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NativeDigitalPlanMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchSize",
                table: "DigitalNewsSyncSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalMinutes",
                table: "DigitalNewsSyncSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LookbackDays",
                table: "DigitalNewsSyncSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QueryCursor",
                table: "DigitalNewsSyncSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OriginalUrlVerified",
                table: "DigitalNewsItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "DigitalNewsItems",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKey",
                table: "DigitalNewsItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DigitalPlanKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phrase = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPlanKeywords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DigitalPlanReads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleHash = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    AgencyName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PlanKind = table.Column<string>(type: "text", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Evidence = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPlanReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalPlanReads_DigitalNewsItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "DigitalNewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalPlanSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AgencyName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    RequiredTerms = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPlanSources", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "DigitalNewsSyncSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatchSize", "IntervalMinutes", "LookbackDays", "QueryCursor" },
                values: new object[] { 12, 30, 365, 0 });

            migrationBuilder.InsertData(
                table: "DigitalPlanKeywords",
                columns: new[] { "Id", "Conditions", "Enabled", "Name", "Phrase", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("0caf78f5-40c9-5fd6-b2f8-60784be9d946"), "chính phủ số, chính quyền số", true, "trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định", "trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định", 16 },
                    { new Guid("183112a3-51f1-5f50-8c08-11e3eef20fe5"), "chính phủ số, chính quyền số", true, "sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công", "sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công", 13 },
                    { new Guid("187f0486-77ed-5935-800d-0ea538e62380"), "chính phủ số, chính quyền số", true, " hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa", "hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa", 11 },
                    { new Guid("1fd91266-e51e-5516-adee-85db39432675"), "Khung kiến trúc số ", true, "Khung kiến trúc số ", "Khung kiến trúc số ", 10 },
                    { new Guid("243c6deb-35ca-5304-8a68-e38f4fcc1ba1"), "chuyển đổi số", true, "Phê duyệt", "chương trình chuyển đổi số", 7 },
                    { new Guid("2d13b6bc-b2b6-54d0-ac6b-18cbb6bf2827"), "chuyển đổi số", true, "Phê duyệt", "Bộ chỉ số chuyển đổi số", 6 },
                    { new Guid("55b6869f-b76b-5e49-8d41-df9b549c5e93"), "chuyển đổi số", true, "Triển khai kế hoạch", "triển khai kế hoạch", 4 },
                    { new Guid("807e04ee-7a01-5070-9842-5df0b25201e1"), "chính phủ số, chính quyền số", true, "cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI", "cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI", 15 },
                    { new Guid("84491507-1514-5140-a88c-70202ce2036d"), "chuyển đổi số", true, "Ban hành đề án", "ban hành đề án chuyển đổi số", 2 },
                    { new Guid("87ba5cdf-eeef-5c79-aea6-bc65121a3c0d"), "chính phủ số, chính quyền số", true, "cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số", "cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số", 19 },
                    { new Guid("aa72ab90-2491-551f-942d-8d9b18eaba1d"), "chuyển đổi số", true, "Ban hành nghị quyết", "ban hành nghị quyết chuyển đổi số", 3 },
                    { new Guid("adb5c0c0-3f43-5ef5-82bf-2addfb073888"), "chuyển đổi số", true, "Phê duyệt", "Kế hoạch chuyển đổi số", 5 },
                    { new Guid("b5989c8d-1086-5141-8cbc-b039826f4ad4"), "chính phủ số, chính quyền số", true, "số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính", "số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính", 18 },
                    { new Guid("b5d4f2e1-dfeb-5efc-9cde-89730f4a944e"), "chuyển đổi số", true, "Ban hành kế hoạch", "ban hành kế hoạch chuyển đổi số", 1 },
                    { new Guid("bf2a0a2f-03dc-5c5a-99f7-bd1bc0b75548"), "kinh tế số", true, "Hỗ trợ doanh nghiệp nhỏ và vừa chuyển đổi số", "Hỗ trợ doanh nghiệp nhỏ và vừa chuyển đổi số", 21 },
                    { new Guid("c6fd3b16-c294-57ca-828c-f9f304b894e7"), "chính phủ số, chính quyền số", true, "cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ", "cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ", 20 },
                    { new Guid("ce9b921f-090f-593f-8235-8cb5b5dc50f4"), "chính phủ số, chính quyền số", true, "hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu", "hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu", 14 },
                    { new Guid("d3eb6d57-b18a-5c4f-89a7-777c6ad63906"), "kinh tế số", true, "Tỷ trọng kinh tế số", "Tỷ trọng kinh tế số", 8 },
                    { new Guid("e20a202e-7637-5bcb-8109-507570b54afe"), "chính phủ số, chính quyền số", true, "cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định", "cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định", 17 },
                    { new Guid("eb93cb04-c498-5c5b-9f3c-f4d909eddc07"), "chính phủ số, chính quyền số", true, "người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến", "người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến", 12 },
                    { new Guid("f306de91-2ae3-56ac-91bf-04676701ec74"), "Dịch vụ công trực tuyến", true, "Dịch vụ công trực tuyến", "Dịch vụ công trực tuyến", 9 }
                });

            migrationBuilder.InsertData(
                table: "DigitalPlanSources",
                columns: new[] { "Id", "AgencyName", "Domain", "Enabled", "Name", "RequiredTerms", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("02538116-b9d5-5713-8f0b-ec96dccca168"), "Bộ Dân tộc và Tôn giáo", "bdttg.gov.vn", true, "Bộ Dân tộc và Tôn giáo", "\"chuyển đổi số\"", 14 },
                    { new Guid("03b89e3d-e325-50eb-912c-3eeedab3164b"), "Bộ Giáo dục và Đào tạo", "moet.gov.vn", true, "Bộ Giáo dục và Đào tạo", "\"chuyển đổi số\"", 12 },
                    { new Guid("09e8da05-062b-59cc-9566-21b96f3d4c6d"), "Tỉnh Nghệ An", "ngheandost.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Nghệ An", "\"chuyển đổi số\"", 74 },
                    { new Guid("0ac7c65c-b7b6-5dba-8ce7-b18b4ae3f70c"), "Tỉnh Hà Tĩnh", "hatinh.gov.vn", true, "Hà Tĩnh", "\"chuyển đổi số\"", 34 },
                    { new Guid("0fa332e5-f255-53d7-a66b-38f9bc3d2564"), "Tỉnh Tuyên Quang", "tuyenquang.gov.vn", true, "Tuyên Quang", "\"chuyển đổi số\"", 49 },
                    { new Guid("122ad42e-e858-5fb7-950e-cc14baf17c61"), "Tỉnh Cao Bằng", "caobang.gov.vn", true, "Cao Bằng", "\"chuyển đổi số\"", 28 },
                    { new Guid("1384a0d5-a894-5f33-a016-b9efa6c6d13f"), "Tỉnh Đắk Lắk", "skhcn.daklak.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Đắk Lắk", "\"chuyển đổi số\"", 68 },
                    { new Guid("1951ab7a-c764-5a0c-be62-e33f2aaf4f84"), "Tỉnh Lạng Sơn", "sokhcn.langson.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Lạng Sơn", "\"chuyển đổi số\"", 60 },
                    { new Guid("1c0fc95b-652b-5d00-ab18-36fab8461cc5"), "Tỉnh Ninh Bình", "sokhcn.ninhbinh.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Ninh Bình", "\"chuyển đổi số\"", 61 },
                    { new Guid("1c4e467b-2c2b-5512-af8f-abf74175614c"), "Tỉnh Nghệ An", "nghean.gov.vn", true, "Nghệ An", "\"chuyển đổi số\"", 41 },
                    { new Guid("209991d0-4a02-514a-aa26-480b80b80590"), "Tỉnh Bắc Ninh", "skhcn.bacninh.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Bắc Ninh", "\"chuyển đổi số\"", 54 },
                    { new Guid("254574ad-9046-55f5-9ccc-2c5b8b9a5fb2"), "Tỉnh Phú Thọ", "sokhoahoccongnghe.phutho.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Phú Thọ", "\"chuyển đổi số\"", 62 },
                    { new Guid("29340d21-7626-58a8-b06c-214129571489"), "Thành phố Cần Thơ", "cantho.gov.vn", true, "Cần Thơ", "\"chuyển đổi số\"", 22 },
                    { new Guid("394da1e9-6c21-57c6-a22f-b73c1b7726b5"), "Bộ Công Thương", "moit.gov.vn", true, "Bộ Công Thương", "\"chuyển đổi số\"", 7 },
                    { new Guid("3b1d123e-28d5-51ec-a4d5-d89a79883670"), "Thành phố Hồ Chí Minh", "dost.hochiminhcity.gov.vn", true, "Sở Khoa học và Công Nghệ Thành phố Hồ Chí Minh", "\"chuyển đổi số\"", 75 },
                    { new Guid("4360becc-4e57-5212-9c7a-97f5de34ceee"), "Tỉnh Điện Biên", "skhcn.dienbien.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Điện Biên", "\"chuyển đổi số\"", 56 },
                    { new Guid("4890bbb6-bb8f-5e8e-b3a8-a3bbfa46dc8d"), "Bộ Công an", "mps.gov.vn", true, "Bộ Công an", "\"chuyển đổi số\"", 2 },
                    { new Guid("4d67d277-ef95-58f5-873b-0f08462af33f"), "Tỉnh Thừa Thiên Huế", "skhcn.hue.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Thừa Thiên Huế", "\"chuyển đổi số\"", 72 },
                    { new Guid("5458bf9d-f2d9-5443-964d-225182d4eacb"), "Tỉnh Điện Biên", "dienbien.gov.vn", true, "Điện Biên", "\"chuyển đổi số\"", 30 },
                    { new Guid("5465973c-2bd8-57e7-9c0d-f5142ef3be36"), "Tỉnh Tuyên Quang", "skhcn.tuyenquang.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Tuyên Quang", "\"chuyển đổi số\"", 65 },
                    { new Guid("581ed27c-0633-5157-b979-36358b48263e"), "Tỉnh Đồng Tháp", "skhcn.dongthap.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Đồng Tháp", "\"chuyển đổi số\"", 82 },
                    { new Guid("58fbd8f6-ef30-5fad-8ea8-e877f41b3eb9"), "Tỉnh Thái Nguyên", "thainguyen.gov.vn", true, "Thái Nguyên", "\"chuyển đổi số\"", 48 },
                    { new Guid("5c1a2cbb-778d-5a6d-976b-f1ac7c86bef9"), "Thành phố Bắc Ninh", "bacninh.gov.vn", true, "Bắc Ninh", "\"chuyển đổi số\"", 26 },
                    { new Guid("5d28f892-f051-5925-b60d-fa0b5a1863ab"), "Bộ Xây dựng", "moc.gov.vn", true, "Bộ Xây dựng", "\"chuyển đổi số\"", 9 },
                    { new Guid("5e31f60f-21fd-5273-9a40-0c866c429570"), "Tỉnh Lạng Sơn", "langson.gov.vn", true, "Lạng Sơn", "\"chuyển đổi số\"", 40 },
                    { new Guid("5ef3314a-7797-54b8-985c-8905284e254d"), "Tỉnh Lai Châu", "skhcn.laichau.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Lai Châu", "\"chuyển đổi số\"", 58 },
                    { new Guid("6044d071-7dc8-55f9-ae71-218c4f6c8479"), "Thành phố Đồng Nai", "dongnai.gov.vn", true, "Đồng Nai", "\"chuyển đổi số\"", 24 },
                    { new Guid("62ffc86a-e07b-5bf9-b7c9-5da30250e09b"), "Tỉnh Sơn La", "sokhcn.sonla.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Sơn La", "\"chuyển đổi số\"", 64 },
                    { new Guid("65f4623d-7d9c-5b30-934e-bb7c4eec61da"), "Tỉnh Ninh Bình", "ninhbinh.gov.vn", true, "Ninh Bình", "\"chuyển đổi số\"", 42 },
                    { new Guid("67274292-db38-593e-9ef8-8ec8031b4e2e"), "Tỉnh Thái Nguyên", "dosttn.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Thái Nguyên", "\"chuyển đổi số\"", 66 },
                    { new Guid("6878a2c2-698b-56b5-aef8-3bc114421b2f"), "Bộ Văn hóa, Thể thao và Du lịch", "bvhttdl.gov.vn", true, "Bộ Văn hóa, Thể thao và Du lịch", "\"chuyển đổi số\"", 10 },
                    { new Guid("6cd1ceeb-0001-5cc4-88cd-5db7dcaf4fc5"), "Thanh tra Chính phủ", "thanhtra.gov.vn", true, "Thanh tra Chính phủ", "\"chuyển đổi số\"", 17 },
                    { new Guid("74fa9b50-2edf-5e80-b22a-b9e411ab8108"), "Tỉnh Đắk Nông", "skhcn.daknong.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Đắk Nông", "\"chuyển đổi số\"", 69 },
                    { new Guid("79ab5f06-70f0-5d2b-b7fc-ea20d4dab159"), "Thành phố Hà Nội", "sokhcn.hanoi.gov.vn", true, "Sở Khoa học và Công Nghệ Thành phố Hà Nội", "\"chuyển đổi số\"", 52 },
                    { new Guid("7a6ee44a-de07-5328-aa1a-500ca3f61ba4"), "Tỉnh Lâm Đồng", "lamdong.gov.vn/sites/skhcn", true, "Sở Khoa học và Công Nghệ Tỉnh Lâm Đồng", "\"chuyển đổi số\"", 85 },
                    { new Guid("7c2d6e0b-0da7-50e6-8c59-a90853bb2669"), "Thành phố Hà Nội", "hanoi.gov.vn", true, "Hà Nội", "\"chuyển đổi số\"", 18 },
                    { new Guid("815f7fda-1890-5000-b8b8-79ad0a570ee5"), "Ngân hàng Nhà nước Việt Nam", "sbv.gov.vn", true, "Ngân hàng Nhà nước Việt Nam", "\"chuyển đổi số\"", 16 },
                    { new Guid("8577c11d-82a6-56f9-9547-d626050677ab"), "Thành phố Hải Phòng", "sokhcn.haiphong.gov.vn", true, "Sở Khoa học và Công Nghệ Thành phố Hải Phòng", "\"chuyển đổi số\"", 53 },
                    { new Guid("86931764-ecd4-5e49-9b0b-0fca5f1cd906"), "Tỉnh Hà Tĩnh", "skhcn.hatinh.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Hà Tĩnh", "\"chuyển đổi số\"", 71 },
                    { new Guid("86faf33d-897e-5f13-b359-f0ace0174cb5"), "Tỉnh Cà Mau", "camau.gov.vn", true, "Cà Mau", "\"chuyển đổi số\"", 29 },
                    { new Guid("8c2508a5-aaa2-5c8f-9164-e3375bcc69d9"), "Bộ Nông nghiệp và Môi trường", "mae.gov.vn", true, "Bộ Nông nghiệp và Môi trường", "\"chuyển đổi số\"", 8 },
                    { new Guid("8c5978ae-000b-5dec-ab70-c33dbea8e007"), "Thành phố Đà Nẵng", "dost.danang.gov.vn", true, "Sở Khoa học và Công Nghệ Thành phố Đà Nẵng", "\"chuyển đổi số\"", 67 },
                    { new Guid("8d15f7c7-9b96-5e14-8df5-ace6c61d9e64"), "Thành phố Quảng Ninh", "quangninh.gov.vn", true, "Quảng Ninh", "\"chuyển đổi số\"", 25 },
                    { new Guid("8f3d5e11-7754-526c-8178-9bc75154372c"), "Tỉnh Lai Châu", "laichau.gov.vn", true, "Lai Châu", "\"chuyển đổi số\"", 37 },
                    { new Guid("932b67cc-a632-5dbf-ac62-afb303954ba7"), "Bộ Khoa học và Công nghệ", "mst.gov.vn", true, "Bộ Khoa học và Công nghệ", "\"chuyển đổi số\"", 11 },
                    { new Guid("93eee9bb-9c0c-527c-97ee-580ee2e5418e"), "Bộ Quốc phòng", "mod.gov.vn", true, "Bộ Quốc phòng", "\"chuyển đổi số\"", 1 },
                    { new Guid("9c95440b-6a88-5d51-ac6d-4425d4c018c6"), "Văn phòng Chính phủ", "vpcp.chinhphu.vn", true, "Văn phòng Chính phủ", "\"chuyển đổi số\"", 15 },
                    { new Guid("a111c80c-4520-535c-9481-8a263de36cf4"), "Bộ Ngoại giao", "mofa.gov.vn", true, "Bộ Ngoại giao", "\"chuyển đổi số\"", 3 },
                    { new Guid("a2924ba5-60b7-5b9d-aef3-4277cd56e2a7"), "Tỉnh Gia Lai", "gialai.gov.vn", true, "Gia Lai", "\"chuyển đổi số\"", 33 },
                    { new Guid("ac9a188e-5a9e-528d-afb5-d0fc611459b3"), "Thành phố Hải Phòng", "haiphong.gov.vn", true, "Hải Phòng", "\"chuyển đổi số\"", 20 },
                    { new Guid("b0f8a3a6-086e-532b-b2cc-b09708e5a688"), "Tỉnh Sơn La", "sonla.gov.vn", true, "Sơn La", "\"chuyển đổi số\"", 46 },
                    { new Guid("b1cd0a74-2657-5943-b309-a753d6926c26"), "Thành phố Huế", "thuathienhue.gov.vn", true, "Huế", "\"chuyển đổi số\"", 23 },
                    { new Guid("b3420df3-7a07-5280-8ca8-e023cb27561b"), "Tỉnh Quảng Trị", "quangtri.gov.vn", true, "Quảng Trị", "\"chuyển đổi số\"", 45 },
                    { new Guid("b44ef28b-de69-555a-9aba-dffff1c9c88c"), "Tỉnh Tây Ninh", "sokhcn.tayninh.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Tây Ninh", "\"chuyển đổi số\"", 83 },
                    { new Guid("b4b9ae3d-e09c-56ff-ab1d-ac8a3b8459e8"), "Tỉnh Lâm Đồng", "lamdong.gov.vn", true, "Lâm Đồng", "\"chuyển đổi số\"", 39 },
                    { new Guid("b505201d-dd98-5687-ad0b-ee9d1861526a"), "Thành phố Cần Thơ", "sokhcn.cantho.gov.vn", true, "Sở Khoa học và Công Nghệ Thành phố Cần Thơ", "\"chuyển đổi số\"", 76 },
                    { new Guid("b6d04da9-fc49-547b-bbd0-8ec6395828c9"), "Tỉnh Quảng Ngãi", "quangngai.gov.vn", true, "Quảng Ngãi", "\"chuyển đổi số\"", 44 },
                    { new Guid("bb51fda7-aa41-53df-b2d3-b26145457d4a"), "Tỉnh Đồng Nai", "dost-dongnai.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Đồng Nai", "\"chuyển đổi số\"", 81 },
                    { new Guid("bcd5330f-2b01-59bd-a166-7a12085c58cd"), "Thành phố Đà Nẵng", "danang.gov.vn", true, "Đà Nẵng", "\"chuyển đổi số\"", 21 },
                    { new Guid("bd33ace7-a541-5a80-b4fa-66a5ccc14102"), "Tỉnh Đắk Lắk", "daklak.gov.vn", true, "Đắk Lắk", "\"chuyển đổi số\"", 31 },
                    { new Guid("bdc40b5f-e4b2-5b17-9eb0-ec1b05179da6"), "Tỉnh Phú Thọ", "phutho.gov.vn", true, "Phú Thọ", "\"chuyển đổi số\"", 43 },
                    { new Guid("be618a84-5f0c-5dd8-aafe-47a141fbdd2d"), "Tỉnh Bình Phước", "skhcn.binhphuoc.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Bình Phước", "\"chuyển đổi số\"", 79 },
                    { new Guid("c69f02ca-c2d3-5e10-9cdc-eac190b4c3b4"), "Thành phố Hồ Chí Minh", "hochiminhcity.gov.vn", true, "TP. Hồ Chí Minh", "\"chuyển đổi số\"", 19 },
                    { new Guid("c72ac9f2-92ba-5479-9407-4ec3fb5402f1"), "Bộ Tài chính", "mof.gov.vn", true, "Bộ Tài chính", "\"chuyển đổi số\"", 6 },
                    { new Guid("c886cf3f-8a96-5aeb-b0c7-cb68169cb280"), "Tỉnh An Giang", "sokhcn.angiang.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh An Giang", "\"chuyển đổi số\"", 77 },
                    { new Guid("cfad0509-b69f-5ae1-a243-7c592b5ec77f"), "Tỉnh Thanh Hóa", "thanhhoa.gov.vn", true, "Thanh Hóa", "\"chuyển đổi số\"", 47 },
                    { new Guid("d44909b3-9f88-5dec-bdce-70840cd76582"), "Bộ Tư pháp", "moj.gov.vn", true, "Bộ Tư pháp", "\"chuyển đổi số\"", 5 },
                    { new Guid("d9b4f85c-b687-5965-9754-ad8a88c8b773"), "Tỉnh Hưng Yên", "sokhcn.hungyen.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Hưng Yên", "\"chuyển đổi số\"", 57 },
                    { new Guid("db686756-3229-5147-a370-8abdd4fad69c"), "Tỉnh Gia Lai", "skhcn.gialai.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Gia Lai", "\"chuyển đổi số\"", 70 },
                    { new Guid("df23df18-b437-588b-adc5-dfdf4c0fb4b3"), "Tỉnh Lào Cai", "skhcn.laocai.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Lào Cai", "\"chuyển đổi số\"", 59 },
                    { new Guid("e163644e-110c-5c05-8fd9-e1486f233871"), "Tỉnh Cao Bằng", "sokhcn.caobang.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Cao Bằng", "\"chuyển đổi số\"", 55 },
                    { new Guid("e1b97440-7401-500c-b54f-afe39f30319e"), "Tỉnh Bến Tre", "dost-bentre.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Bến Tre", "\"chuyển đổi số\"", 78 },
                    { new Guid("e49ffee0-1e8a-5442-84ac-27b1e7b4dfd8"), "Tỉnh Khánh Hòa", "khanhhoa.gov.vn", true, "Khánh Hòa", "\"chuyển đổi số\"", 36 },
                    { new Guid("e556e219-102e-5401-a65e-da8e5be84d0a"), "Tỉnh Lào Cai", "laocai.gov.vn", true, "Lào Cai", "\"chuyển đổi số\"", 38 },
                    { new Guid("e5d6ca53-f593-5de0-8d06-ada571bb8cc1"), "Tỉnh Vĩnh Long", "vinhlong.gov.vn", true, "Vĩnh Long", "\"chuyển đổi số\"", 51 },
                    { new Guid("e825f0b7-e243-5d45-a578-d48bde3113d9"), "Tỉnh Hưng Yên", "hungyen.gov.vn", true, "Hưng Yên", "\"chuyển đổi số\"", 35 },
                    { new Guid("e9a14935-9d87-50c1-a3ab-62845a1d3ff2"), "Bộ Nội vụ", "moha.gov.vn", true, "Bộ Nội vụ", "\"chuyển đổi số\"", 4 },
                    { new Guid("ebf9a805-8eb4-508b-b686-ce7e0d6e9128"), "Tỉnh Tây Ninh", "tayninh.gov.vn", true, "Tây Ninh", "\"chuyển đổi số\"", 50 },
                    { new Guid("f081615d-e321-5869-9d96-b59d3435fd3f"), "Tỉnh An Giang", "angiang.gov.vn", true, "An Giang", "\"chuyển đổi số\"", 27 },
                    { new Guid("f11286ef-4f9b-5108-a3a0-a9d65cdac76d"), "Tỉnh Đồng Tháp", "dongthap.gov.vn", true, "Đồng Tháp", "\"chuyển đổi số\"", 32 },
                    { new Guid("f53492b1-0c26-5ee0-8686-993c5536b2e1"), "Bộ Y tế", "moh.gov.vn", true, "Bộ Y tế", "\"chuyển đổi số\"", 13 },
                    { new Guid("f862ea29-b6b6-5020-86a4-4d4a3492a734"), "Tỉnh Cà Mau", "sokhcn.camau.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Cà Mau", "\"chuyển đổi số\"", 80 },
                    { new Guid("f886e4af-c109-582e-8ea7-97f11ad9eb11"), "Tỉnh Quảng Ninh", "quangninh.gov.vn/so/sokhoahoccongnghe", true, "Sở Khoa học và Công Nghệ Tỉnh Quảng Ninh", "\"chuyển đổi số\"", 63 },
                    { new Guid("fb0686a2-8297-5bab-9491-3e803f80f18c"), "Tỉnh Khánh Hòa", "skhcn.khanhhoa.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Khánh Hòa", "\"chuyển đổi số\"", 73 },
                    { new Guid("fbda2393-816a-5208-8393-43903b296409"), "Tỉnh Vĩnh Long", "skhcn.vinhlong.gov.vn", true, "Sở Khoa học và Công Nghệ Tỉnh Vĩnh Long", "\"chuyển đổi số\"", 84 }
                });

            // Preserve old imported articles and AI histories; stop only idle legacy Sheet jobs.
            migrationBuilder.Sql("""
                UPDATE "AiInboxItems" SET "Status"='NeedsReview', "Error"='Bài đã chuyển sang Tiện ích > Thông tin chuyển đổi số. Bấm Đọc tại đó để đánh giá kế hoạch.'
                WHERE "Origin" LIKE 'Google Sheets:%' AND "Status" IN ('TitleQueued','ReadQueued')
                  AND ("LeaseUntil" IS NULL OR "LeaseUntil" < CURRENT_TIMESTAMP);
                UPDATE "DigitalNewsSyncSettings" SET "NextRunAt"=NULL WHERE "LeaseUntil" IS NULL OR "LeaseUntil" < CURRENT_TIMESTAMP;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalNewsItems_TitleKey",
                table: "DigitalNewsItems",
                column: "TitleKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPlanReads_ItemId_TitleHash",
                table: "DigitalPlanReads",
                columns: new[] { "ItemId", "TitleHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalPlanKeywords");

            migrationBuilder.DropTable(
                name: "DigitalPlanReads");

            migrationBuilder.DropTable(
                name: "DigitalPlanSources");

            migrationBuilder.DropIndex(
                name: "IX_DigitalNewsItems_TitleKey",
                table: "DigitalNewsItems");

            migrationBuilder.DropColumn(
                name: "BatchSize",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.DropColumn(
                name: "IntervalMinutes",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.DropColumn(
                name: "LookbackDays",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.DropColumn(
                name: "QueryCursor",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.DropColumn(
                name: "OriginalUrlVerified",
                table: "DigitalNewsItems");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "DigitalNewsItems");

            migrationBuilder.DropColumn(
                name: "TitleKey",
                table: "DigitalNewsItems");
        }
    }
}
