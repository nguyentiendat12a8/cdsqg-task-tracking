using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SerializeAiInboxQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiInboxQueueState",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Owner = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NextReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInboxQueueState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AiInboxQueueState",
                columns: new[] { "Id", "LeaseUntil", "NextReadAt", "Owner" },
                values: new object[] { 1, null, new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiInboxQueueState");
        }
    }
}
