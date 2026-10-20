using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAllAiGoalKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000018"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, cashless transactions, vietnam", "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp, giao dịch phi tiền mặt" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000020"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "fiber optic, ftth, fixed broadband, households, broadband penetration, fiber coverage, vietnam", "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet, phủ cáp quang" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000021"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "bank account, transaction account, adults, banked population, financial inclusion, account ownership, vietnam", "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng, tài khoản thanh toán" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000022"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "digital signature, electronic signature, personal digital signature, e-signature, digital certificate, vietnam", "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số cá nhân, vnpt smartca, viettel ca" });

            migrationBuilder.InsertData(
                table: "AiGoalKeywords",
                columns: new[] { "Id", "CreatedAt", "ExpectedUnit", "ExtractionPrompt", "GoalCode", "GoalTitle", "KeywordsEn", "KeywordsVi", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).", "MT-01", "100% hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa", "digitization, digitized administrative records, public administrative services, digitized results, digital archives, vietnam", "số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử, bưu chính công ích", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).", "MT-02", "99% người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến", "user satisfaction, citizen satisfaction, satisfaction rate, online public services, national public service portal, vietnam", "đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ bộ, ngành, địa phương sử dụng nền tảng số dùng chung quốc gia phục vụ chỉ đạo điều hành và dịch vụ công (%).", "MT-03", "100% bộ, ngành, địa phương sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công", "shared digital platforms, ministries, provinces, national digital platforms, public service delivery, data exchange, vietnam", "nền tảng số dùng chung, bộ ngành địa phương, chỉ đạo điều hành, dịch vụ công, trục liên thông văn bản, ndxp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ hệ thống thông tin có kết nối, chia sẻ dữ liệu qua nền tảng tích hợp, chia sẻ dữ liệu (%).", "MT-04", "100% hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu", "information systems, data sharing, data exchange, ndxp, national data exchange platform, interoperability, vietnam", "kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, tích hợp dữ liệu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ cơ sở dữ liệu dùng chung quốc gia, ngành, địa phương được chuẩn hóa và sẵn sàng kết nối, khai thác AI (%).", "MT-05", "100% cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI", "shared databases, national databases, standardized data, ai readiness, artificial intelligence, open data, vietnam", "cơ sở dữ liệu dùng chung, csdl quốc gia, csdl chuyên ngành, chuẩn hóa dữ liệu, sẵn sàng khai thác ai, trí tuệ nhân tạo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ trung tâm điều hành thông minh (IOC) cấp tỉnh được nâng cấp tích hợp AI (%).", "MT-06", "100% trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định", "intelligent operations center, ioc, provincial operations center, ai integration, predictive analytics, smart city, vietnam", "trung tâm điều hành thông minh, ioc, cấp tỉnh, tích hợp ai, dự báo cảnh báo, giám sát ra quyết định", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử (%).", "MT-07", "100% cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định", "civil servants, public employees, electronic personnel records, civil servant database, ministry of home affairs, vietnam", "cán bộ công chức viên chức, hồ sơ điện tử, quản lý cán bộ, cơ sở dữ liệu cán bộ công chức, cbccvc, bộ nội vụ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính (%).", "MT-08", "70% số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính", "statistical indicators, administrative data sources, national statistical system, general statistics office, gso, vietnam", "chỉ tiêu thống kê, dữ liệu hành chính, hệ thống chỉ tiêu, tổng cục thống kê, cục thống kê, số liệu hành chính", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng cập nhật kỹ năng số (%).", "MT-09", "100% cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số", "digital skills training, digital literacy, civil servants training, public sector digital skills, government workforce, vietnam", "đào tạo kỹ năng số, bồi dưỡng kiến thức số, cán bộ công chức, chuyển đổi số công vụ, tập huấn kỹ năng số", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ cán bộ chuyên trách chuyển đổi số, CNTT được đào tạo, bồi dưỡng nâng cao công nghệ số (%).", "MT-10", "100% cán bộ chuyên trách về chuyển đổi số, công nghệ thông tin được đào tạo, bồi dưỡng, cập nhật công nghệ số", "it specialists, digital transformation officers, specialized it training, technology capacity, vietnam", "chuyên trách cntt, chuyên trách chuyển đổi số, bồi dưỡng công nghệ số, cán bộ công nghệ thông tin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ cán bộ, công chức, viên chức có tài khoản và được tập huấn sử dụng trợ lý AI công vụ (%).", "MT-11", "100% cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ", "public service ai assistant, virtual assistant for civil servants, ai assistant accounts, public sector ai, vietnam", "trợ lý ai công vụ, trợ lý ảo công vụ, tài khoản trợ lý ai, đào tạo trợ lý ảo, ai phục vụ cán bộ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "doanh nghiệp", "Xác định số lượng doanh nghiệp nhỏ và vừa (SME) được hỗ trợ chuyển đổi số (đơn vị: doanh nghiệp).", "MT-14", "Hỗ trợ tối thiểu 500.000 doanh nghiệp nhỏ và vừa chuyển đổi số", "smes, small and medium enterprises, enterprise digital transformation, smedx, business support, vietnam", "doanh nghiệp nhỏ và vừa, sme, chuyển đổi số doanh nghiệp, smedx, hỗ trợ doanh nghiệp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sàn", "Xác định số lượng sàn dữ liệu / trung tâm giao dịch dữ liệu được đưa vào vận hành (đơn vị: sàn).", "MT-15", "Phát triển, thúc đẩy và đưa vào sử dụng tối thiểu 5 sàn dữ liệu, bảo đảm an toàn, tuân thủ quy định của pháp luật", "data exchange platform, data marketplace, data trading, data exchanges, vietnam", "sàn dữ liệu, trung tâm giao dịch dữ liệu, thị trường dữ liệu, data exchange, sàn giao dịch dữ liệu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000017"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ doanh nghiệp nhỏ và vừa ngành thương mại tham gia các sàn thương mại điện tử (%).", "MT-17", "Phấn đấu đạt 45% số doanh nghiệp nhỏ và vừa hoạt động trong lĩnh vực thương mại tham gia các sàn thương mại điện tử lớn trong và ngoài nước", "smes on ecommerce platforms, ecommerce marketplace participation, online retail smes, e-marketplace, vietnam", "sme thương mại, sàn thương mại điện tử, tham gia sàn tmđt, shopee, lazada, tiki, alibaba, amazon, bán hàng trực tuyến", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000019"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ doanh nghiệp nhỏ và vừa tiếp cận hoặc sử dụng giải pháp AI / mô hình như một dịch vụ (%).", "MT-19", "Trên 40% doanh nghiệp nhỏ và vừa tiếp cận, sử dụng nền tảng “mô hình như một dịch vụ”, tác nhân AI hoặc giải pháp AI dùng chung", "model as a service, maas, ai agents, shared ai solutions, smes adopting ai, ai adoption rate, vietnam", "mô hình như một dịch vụ, maas, tác nhân ai, ai agent, giải pháp ai dùng chung, sme ứng dụng ai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000023"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ người dân được lập sổ / hồ sơ sức khỏe điện tử (%).", "MT-23", "Phấn đấu tỷ lệ người dân được lập sổ sức khỏe điện tử đạt 100%", "electronic health record, ehr, personal health record, digital healthcare, ministry of health, vietnam", "sổ sức khỏe điện tử, hồ sơ sức khỏe điện tử, y tế số, vneid sức khỏe, bộ y tế, bảo hiểm y tế", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000024"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lượt người", "Xác định số lượt người trong độ tuổi lao động được đào tạo, bồi dưỡng kỹ năng số cơ bản (đơn vị: lượt người).", "MT-24", "Đào tạo, bồi dưỡng, tập huấn kỹ năng số cơ bản cho tối thiểu 10.000.000 lượt người trong độ tuổi lao động", "basic digital skills, working age population, digital skills training, digital literacy training, participants, vietnam", "kỹ năng số cơ bản, độ tuổi lao động, tập huấn kỹ năng số, phổ cập kỹ năng số, lượt người", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000026"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).", "MT-26", "Quy mô kinh tế số đạt tối thiểu 50% GDP, kinh tế số trở thành một trong những động lực phát triển chủ yếu của nền kinh tế", "digital economy, gdp, share of gdp, 50% gdp, gross domestic product, digital economic growth, vietnam", "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, 50% gdp, động lực phát triển", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000026"));

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000018"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, vietnam", "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000020"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "fiber optic, ftth, fixed broadband, households, broadband penetration, vietnam", "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000021"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "bank account, transaction account, adults, banked population, financial inclusion, vietnam", "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng" });

            migrationBuilder.UpdateData(
                table: "AiGoalKeywords",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000022"),
                columns: new[] { "KeywordsEn", "KeywordsVi" },
                values: new object[] { "digital signature, electronic signature, personal digital signature, e-signature, vietnam", "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số" });
        }
    }
}
