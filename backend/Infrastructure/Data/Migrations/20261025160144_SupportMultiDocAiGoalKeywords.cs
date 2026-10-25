using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultiDocAiGoalKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiGoalKeywords_GoalCode",
                table: "AiGoalKeywords");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "AiGoalKeywords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("12660000-0000-0000-0000-000000001266"));

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000001"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000002"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000003"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000004"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000005"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000006"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000007"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000008"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000009"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000010"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000011"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000012"),
                column: "DocumentId",
                value: new Guid("12660000-0000-0000-0000-000000001266"));

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000013"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000014"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000015"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000016"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000017"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000018"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000019"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000020"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000021"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000022"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000023"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000024"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000025"),
                column: "DocumentId",
                value: new Guid("12660000-0000-0000-0000-000000001266"));

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000026"),
                columns: new[] { "DocumentId", "KeywordsEn" },
                values: new object[] { new Guid("12660000-0000-0000-0000-000000001266"), "" });

            migrationBuilder.InsertData(
                table: "AiGoalKeywords",
                columns: new[] { "Id", "CreatedAt", "DocumentId", "ExpectedUnit", "ExtractionPrompt", "GoalCode", "GoalTitle", "KeywordsEn", "KeywordsVi", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b2000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện (%). Chỉ lấy số liệu thực tế đã công bố, không lấy ngưỡng mục tiêu.", "MT-01", "Tỉ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện.", "", "dịch vụ công trực tuyến toàn trình, dvctt toàn trình, thủ tục hành chính đủ điều kiện, tthc đủ điều kiện, dịch vụ công toàn trình, cung cấp trực tuyến toàn trình", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ thông tin, giấy tờ trong TTHC về sản xuất kinh doanh chỉ phải cung cấp một lần cho cơ quan nhà nước (%).", "MT-02", "Tỉ lệ thông tin, giấy tờ, tài liệu trong các thủ tục hành chính liên quan đến hoạt động sản xuất, kinh doanh chỉ cung cấp một lần cho cơ quan hành chính nhà nước.", "", "chỉ cung cấp một lần, sản xuất kinh doanh, thủ tục hành chính doanh nghiệp, giấy tờ tài liệu một lần, chia sẻ thông tin thủ tục, cung cấp một lần", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).", "MT-03", "Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.", "", "đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ nền tảng số quốc gia, nền tảng dùng chung phục vụ Chính phủ số hoàn thành và đưa vào sử dụng thống nhất (%).", "MT-04", "Tỉ lệ các nền tảng số quốc gia, nền tảng số dùng chung của ngành, lĩnh vực phục vụ Chính phủ số được hoàn thành và đưa vào sử dụng thống nhất từ Trung ương đến địa phương theo đúng kế hoạch.", "", "nền tảng số quốc gia, nền tảng số dùng chung, chính phủ số, hoàn thành đưa vào sử dụng, sử dụng thống nhất, nền tảng ngành lĩnh vực", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ hồ sơ công việc tại cấp bộ, tỉnh, xã được xử lý trên môi trường điện tử và có dùng chữ ký số cá nhân (%).", "MT-05", "Tỉ lệ hồ sơ công việc tại cấp bộ, tỉnh, cấp xã được xử lý trên môi trường điện tử (trừ hồ sơ công việc thuộc phạm vi bí mật nhà nước) và sử dụng chữ ký số cá nhân để giải quyết công việc.", "", "hồ sơ công việc, môi trường điện tử, cấp bộ, cấp tỉnh, cấp xã, chữ ký số cá nhân, xử lý văn bản điện tử, hệ thống quản lý văn bản", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo (%).", "MT-06", "Tỉ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo.", "", "chế độ báo cáo, hệ thống thông tin báo cáo, hệ thống báo cáo quốc gia, báo cáo điện tử, số hóa chế độ báo cáo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao được theo dõi, giám sát trên môi trường điện tử (%).", "MT-07", "Tỉ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao cho các bộ, ngành, địa phương được theo dõi, giám sát, đánh giá trên môi trường điện tử.", "", "nhiệm vụ chính phủ giao, thủ tướng chính phủ giao, theo dõi giám sát đánh giá, môi trường điện tử, hệ thống theo dõi nhiệm vụ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 3 về mức độ trưởng thành quản trị dữ liệu (%).", "MT-08", "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.", "", "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 3, mức độ trưởng thành dữ liệu, đánh giá quản trị dữ liệu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng kỹ năng số cơ bản (%).", "MT-09", "Tỉ lệ cán bộ, công chức, viên chức được đào tạo kỹ năng số cơ bản.", "", "đào tạo kỹ năng số, kỹ năng số cơ bản, cán bộ công chức viên chức, bồi dưỡng kỹ năng số, phổ cập kỹ năng số công chức", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (%).", "MT-10", "Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.", "", "chuyên trách cntt, chuyên trách chuyển đổi số, chứng chỉ quản trị dữ liệu, đào tạo quản trị dữ liệu cơ bản", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (giai đoạn 2028-2030) (%).", "MT-11", "Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.", "", "đánh giá hài lòng dvctt, mức độ hài lòng người dân, cổng dịch vụ công, sự hài lòng dịch vụ công trực tuyến", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, có AI hỗ trợ cá thể hóa theo mốc sự kiện (%).", "MT-12", "Tỉ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, được AI hỗ trợ “cá thể hóa”, thông báo hoặc gợi ý hành động theo các mốc sự kiện quan trọng trong cuộc đời.", "", "dịch vụ công thiết yếu, dịch vụ công chủ động, ai cá thể hóa, gợi ý hành động, mốc sự kiện cuộc đời, trợ lý ai dịch vụ công", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng tiêu chuẩn, định dạng mở (%).", "MT-13", "Tỉ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng chuẩn.", "", "dữ liệu mở, open data, dữ liệu mở đúng chuẩn, cổng dữ liệu quốc gia, data.gov.vn, cung cấp dữ liệu mở", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).", "MT-14", "Tỉ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa.", "", "số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ khai thác, tái sử dụng thông tin, dữ liệu đã số hóa trong giải quyết TTHC, dịch vụ công (%).", "MT-15", "Tỉ lệ khai thác, sử dụng lại thông tin, dữ liệu đã được số hóa trong giải quyết thủ tục hành chính, dịch vụ công.", "", "khai thác dữ liệu số hóa, sử dụng lại thông tin, tái sử dụng dữ liệu, số hóa trong tthc, không yêu cầu nộp lại giấy tờ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu (%).", "MT-16", "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu.", "", "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 4, mức độ trưởng thành dữ liệu cấp 4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000017"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu (%).", "MT-17", "Tỉ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu.", "", "trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 5, mức độ trưởng thành dữ liệu tối ưu cấp 5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000018"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu (%).", "MT-18", "Tỉ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu.", "", "kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, dịch vụ chia sẻ dữ liệu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000019"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI phục vụ công tác chỉ đạo, điều hành (%).", "MT-19", "Tỉ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI để phục vụ công tác chỉ đạo, điều hành.", "", "ứng dụng ai, trí tuệ nhân tạo, chỉ đạo điều hành, trợ lý ảo, ai phục vụ điều hành, cơ quan tổ chức sử dụng ai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000020"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ đội ngũ lãnh đạo của cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học kỹ thuật, chuyển đổi số (%).", "MT-20", "Tỉ lệ đội ngũ lãnh đạo của từng cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học, kỹ thuật, chuyển đổi số phù hợp với yêu cầu nhiệm vụ.", "", "đội ngũ lãnh đạo, lãnh đạo chuyển đổi số, chuyên môn kỹ thuật, kinh nghiệm khoa học kỹ thuật chuyển đổi số, cán bộ lãnh đạo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000021"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (mục tiêu 2028-2030) (%).", "MT-21", "Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.", "", "chuyên trách cntt chuyển đổi số, chứng chỉ quản trị dữ liệu cơ bản, đào tạo chuyên trách quản trị dữ liệu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2000000-0000-0000-0000-000000000022"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("26290000-0000-0000-0000-000000002629"), "%", "Xác định tỷ lệ hệ thống thông tin Chính phủ số được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ và kiểm tra định kỳ (%).", "MT-22", "Tỉ lệ các hệ thống thông tin phục vụ phát triển Chính phủ số phải được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ trước khi đưa vào vận hành, sử dụng; được kiểm tra, đánh giá an ninh mạng định kỳ.", "", "an ninh mạng theo cấp độ, an toàn thông tin theo cấp độ, phê duyệt hồ sơ cấp độ, kiểm tra an ninh mạng định kỳ, bảo đảm an toàn thông tin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiGoalKeywords_DocumentId_GoalCode",
                table: "AiGoalKeywords",
                columns: new[] { "DocumentId", "GoalCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiGoalKeywords_DocumentId_GoalCode",
                table: "AiGoalKeywords");

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000022"));

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "AiGoalKeywords");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000001"),
                column: "KeywordsEn",
                value: "digitization, digitized administrative records, public administrative services, digitized results, digital archives, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000002"),
                column: "KeywordsEn",
                value: "user satisfaction, citizen satisfaction, satisfaction rate, online public services, national public service portal, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000003"),
                column: "KeywordsEn",
                value: "shared digital platforms, ministries, provinces, national digital platforms, public service delivery, data exchange, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000004"),
                column: "KeywordsEn",
                value: "information systems, data sharing, data exchange, ndxp, national data exchange platform, interoperability, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000005"),
                column: "KeywordsEn",
                value: "shared databases, national databases, standardized data, ai readiness, artificial intelligence, open data, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000006"),
                column: "KeywordsEn",
                value: "intelligent operations center, ioc, provincial operations center, ai integration, predictive analytics, smart city, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000007"),
                column: "KeywordsEn",
                value: "civil servants, public employees, electronic personnel records, civil servant database, ministry of home affairs, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000008"),
                column: "KeywordsEn",
                value: "statistical indicators, administrative data sources, national statistical system, general statistics office, gso, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000009"),
                column: "KeywordsEn",
                value: "digital skills training, digital literacy, civil servants training, public sector digital skills, government workforce, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000010"),
                column: "KeywordsEn",
                value: "it specialists, digital transformation officers, specialized it training, technology capacity, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000011"),
                column: "KeywordsEn",
                value: "public service ai assistant, virtual assistant for civil servants, ai assistant accounts, public sector ai, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000013"),
                column: "KeywordsEn",
                value: "digital economy, gdp, share of gdp, gross domestic product, digital economy share, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000014"),
                column: "KeywordsEn",
                value: "smes, small and medium enterprises, enterprise digital transformation, smedx, business support, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000015"),
                column: "KeywordsEn",
                value: "data exchange platform, data marketplace, data trading, data exchanges, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000016"),
                column: "KeywordsEn",
                value: "e-commerce, ecommerce, retail, consumer services, b2c, online retail share, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000017"),
                column: "KeywordsEn",
                value: "smes on ecommerce platforms, ecommerce marketplace participation, online retail smes, e-marketplace, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000018"),
                column: "KeywordsEn",
                value: "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, cashless transactions, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000019"),
                column: "KeywordsEn",
                value: "model as a service, maas, ai agents, shared ai solutions, smes adopting ai, ai adoption rate, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000020"),
                column: "KeywordsEn",
                value: "fiber optic, ftth, fixed broadband, households, broadband penetration, fiber coverage, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000021"),
                column: "KeywordsEn",
                value: "bank account, transaction account, adults, banked population, financial inclusion, account ownership, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000022"),
                column: "KeywordsEn",
                value: "digital signature, electronic signature, personal digital signature, e-signature, digital certificate, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000023"),
                column: "KeywordsEn",
                value: "electronic health record, ehr, personal health record, digital healthcare, ministry of health, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000024"),
                column: "KeywordsEn",
                value: "basic digital skills, working age population, digital skills training, digital literacy training, participants, vietnam");

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000026"),
                column: "KeywordsEn",
                value: "digital economy, gdp, share of gdp, 50% gdp, gross domestic product, digital economic growth, vietnam");

            migrationBuilder.CreateIndex(
                name: "IX_AiGoalKeywords_GoalCode",
                table: "AiGoalKeywords",
                column: "GoalCode",
                unique: true);
        }
    }
}
