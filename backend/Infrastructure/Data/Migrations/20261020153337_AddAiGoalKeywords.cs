using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiGoalKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiGoalKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GoalTitle = table.Column<string>(type: "text", nullable: false),
                    KeywordsVi = table.Column<string>(type: "text", nullable: false),
                    KeywordsEn = table.Column<string>(type: "text", nullable: false),
                    ExpectedUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExtractionPrompt = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiGoalKeywords", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AiGoalKeywords",
                columns: new[] { "Id", "CreatedAt", "ExpectedUnit", "ExtractionPrompt", "GoalCode", "GoalTitle", "KeywordsEn", "KeywordsVi", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rank", "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).", "MT-12", "Việt Nam thuộc nhóm 50 nước dẫn đầu trên thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)", "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam", "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).", "MT-13", "Quy mô kinh tế số đạt tối thiểu 30% GDP", "digital economy, gdp, share of gdp, gross domestic product, digital economy share, vietnam", "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%).", "MT-16", "Tỷ trọng doanh thu thương mại điện tử so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng đạt từ 15 - 17%", "e-commerce, ecommerce, retail, consumer services, b2c, online retail share, vietnam", "thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000018"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lần GDP", "Xác định tỷ lệ giá trị thanh toán không dùng tiền mặt so với GDP (số lần GDP).", "MT-18", "Giá trị thanh toán không dùng tiền mặt gấp 30 lần GDP", "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, vietnam", "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000020"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ hộ gia đình có kết nối mạng băng rộng cáp quang (FTTH) (%).", "MT-20", "Hạ tầng mạng băng rộng cáp quang phủ 100% hộ gia đình", "fiber optic, ftth, fixed broadband, households, broadband penetration, vietnam", "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000021"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ dân số từ 15 tuổi trở lên có tài khoản thanh toán / giao dịch tại ngân hàng (%).", "MT-21", "Tỷ lệ dân số từ 15 tuổi trở lên có tài khoản giao dịch tại ngân hàng hoặc tổ chức được phép khác đạt tối thiểu 95%", "bank account, transaction account, adults, banked population, financial inclusion, vietnam", "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000022"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%", "Xác định tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân (%).", "MT-22", "Tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân đạt trên 70%", "digital signature, electronic signature, personal digital signature, e-signature, vietnam", "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1000000-0000-0000-0000-000000000025"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rank", "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).", "MT-25", "Việt Nam thuộc nhóm 30 nước dẫn đầu thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)", "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam", "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiGoalKeywords_GoalCode",
                table: "AiGoalKeywords",
                column: "GoalCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiGoalKeywords");
        }
    }
}
