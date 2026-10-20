using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPublishedReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProgressLogId",
                table: "AiGoalDataLinks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiGoalDataLinks_ProgressLogId",
                table: "AiGoalDataLinks",
                column: "ProgressLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiGoalDataLinks_ProgressLogs_ProgressLogId",
                table: "AiGoalDataLinks",
                column: "ProgressLogId",
                principalTable: "ProgressLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiGoalDataLinks_ProgressLogs_ProgressLogId",
                table: "AiGoalDataLinks");

            migrationBuilder.DropIndex(
                name: "IX_AiGoalDataLinks_ProgressLogId",
                table: "AiGoalDataLinks");

            migrationBuilder.DropColumn(
                name: "ProgressLogId",
                table: "AiGoalDataLinks");
        }
    }
}
