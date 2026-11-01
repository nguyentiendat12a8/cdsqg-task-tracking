using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InternetGoalAgencyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear",
                table: "AiDataSuggestions");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetAgencyId",
                table: "AiDataSuggestions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyIdsJson",
                table: "AiAlertFeeds",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "MetricKeysJson",
                table: "AiAlertFeeds",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page",
                table: "AiDataSuggestions",
                columns: new[] { "DocumentId", "MetricKey", "DataYear", "Value", "Page" },
                unique: true,
                filter: "\"TargetAgencyId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page_~",
                table: "AiDataSuggestions",
                columns: new[] { "DocumentId", "MetricKey", "DataYear", "Value", "Page", "TargetAgencyId" },
                unique: true,
                filter: "\"TargetAgencyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear",
                table: "AiDataSuggestions",
                columns: new[] { "MetricKey", "DataYear" },
                unique: true,
                filter: "\"IsCurrent\" = TRUE AND \"TargetAgencyId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear_TargetAgencyId",
                table: "AiDataSuggestions",
                columns: new[] { "MetricKey", "DataYear", "TargetAgencyId" },
                unique: true,
                filter: "\"IsCurrent\" = TRUE AND \"TargetAgencyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiDataSuggestions_TargetAgencyId",
                table: "AiDataSuggestions",
                column: "TargetAgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiDataSuggestions_Agencies_TargetAgencyId",
                table: "AiDataSuggestions",
                column: "TargetAgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiDataSuggestions_Agencies_TargetAgencyId",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_DocumentId_MetricKey_DataYear_Value_Page_~",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_MetricKey_DataYear_TargetAgencyId",
                table: "AiDataSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiDataSuggestions_TargetAgencyId",
                table: "AiDataSuggestions");

            migrationBuilder.DropColumn(
                name: "TargetAgencyId",
                table: "AiDataSuggestions");

            migrationBuilder.DropColumn(
                name: "AgencyIdsJson",
                table: "AiAlertFeeds");

            migrationBuilder.DropColumn(
                name: "MetricKeysJson",
                table: "AiAlertFeeds");

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
        }
    }
}
