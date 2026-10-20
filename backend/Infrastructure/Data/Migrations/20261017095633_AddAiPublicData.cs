using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPublicData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiDataSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MetricKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    NextCheckAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastStatus = table.Column<string>(type: "text", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiSourceDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: false),
                    PagesJson = table.Column<string>(type: "text", nullable: false),
                    RetrievedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSourceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSourceDocuments_AiDataSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "AiDataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AiDataSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricKey = table.Column<string>(type: "text", nullable: false),
                    DataYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodLabel = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    Quote = table.Column<string>(type: "text", nullable: false),
                    Page = table.Column<int>(type: "integer", nullable: false),
                    Methodology = table.Column<string>(type: "text", nullable: false),
                    Warnings = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: false),
                    ReviewNote = table.Column<string>(type: "text", nullable: false),
                    AcceptedValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDataSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiDataSuggestions_AiSourceDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "AiSourceDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AiGoalDataLinks",
                columns: table => new
                {
                    GoalTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiGoalDataLinks", x => new { x.GoalTaskId, x.SuggestionId });
                    table.ForeignKey(
                        name: "FK_AiGoalDataLinks_AiDataSuggestions_SuggestionId",
                        column: x => x.SuggestionId,
                        principalTable: "AiDataSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AiGoalDataLinks_GoalTaskItems_GoalTaskId",
                        column: x => x.GoalTaskId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AiDataSources",
                columns: new[] { "Id", "Enabled", "IntervalHours", "LastCheckedAt", "LastError", "LastStatus", "LeaseUntil", "MetricKey", "Name", "NextCheckAt", "Url" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), false, 168, null, "", "Chưa thu thập", null, "EGDI_RANK", "UN E-Government Survey (trang báo cáo mẫu 2024)", null, "https://desapublications.un.org/publications/un-e-government-survey-2024" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), false, 168, null, "", "Chưa thu thập", null, "DIGITAL_GDP_SHARE", "Cục Thống kê - Tài khoản quốc gia", null, "https://www.nso.gov.vn/tai-khoan-quoc-gia/" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), false, 168, null, "", "Chưa thu thập", null, "ECOM_RETAIL_SHARE", "Sách trắng TMĐT (PDF mẫu 2022, cần chọn kỳ mới)", null, "https://moit.gov.vn/upload/2005517/fck/files/BCTMDT2022-final-pdf_08c88.PDF" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSources_Enabled_NextCheckAt",
                table: "AiDataSources",
                columns: new[] { "Enabled", "NextCheckAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page",
                table: "AiDataSuggestions",
                columns: new[] { "DocumentId", "MetricKey", "DataYear", "Value", "Page" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear",
                table: "AiDataSuggestions",
                columns: new[] { "MetricKey", "DataYear" },
                unique: true,
                filter: "\"IsCurrent\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_AiGoalDataLinks_SuggestionId",
                table: "AiGoalDataLinks",
                column: "SuggestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSourceDocuments_SourceId_Url_ContentHash",
                table: "AiSourceDocuments",
                columns: new[] { "SourceId", "Url", "ContentHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiGoalDataLinks");

            migrationBuilder.DropTable(
                name: "AiDataSuggestions");

            migrationBuilder.DropTable(
                name: "AiSourceDocuments");

            migrationBuilder.DropTable(
                name: "AiDataSources");
        }
    }
}
