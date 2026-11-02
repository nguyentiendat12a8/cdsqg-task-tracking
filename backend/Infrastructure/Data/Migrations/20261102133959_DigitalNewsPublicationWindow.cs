using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DigitalNewsPublicationWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "PublishedFrom",
                table: "DigitalNewsSyncSettings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PublishedTo",
                table: "DigitalNewsSyncSettings",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DigitalNewsSyncSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PublishedFrom", "PublishedTo" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedFrom",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.DropColumn(
                name: "PublishedTo",
                table: "DigitalNewsSyncSettings");
        }
    }
}
