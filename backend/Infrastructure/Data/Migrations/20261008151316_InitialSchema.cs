using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ContactPersons = table.Column<string>(type: "text", nullable: false),
                    PlanFiles = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agencies_Agencies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataImportLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImportedBy = table.Column<string>(type: "text", nullable: false),
                    TotalGoalsCreated = table.Column<int>(type: "integer", nullable: false),
                    TotalTasksCreated = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SummaryNotes = table.Column<string>(type: "text", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImportLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Signer = table.Column<string>(type: "text", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeResolution = table.Column<string>(type: "text", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    AttachmentPath = table.Column<string>(type: "text", nullable: true),
                    AttachmentFilePaths = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    IssuingAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuingAgencyName = table.Column<string>(type: "text", nullable: true),
                    DraftingAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    DraftingAgencyName = table.Column<string>(type: "text", nullable: true),
                    SignerName = table.Column<string>(type: "text", nullable: true),
                    SignerTitle = table.Column<string>(type: "text", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EffectStatus = table.Column<string>(type: "text", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: true),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    AttachmentsJson = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GoalTaskItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Section = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "text", nullable: true),
                    IsOngoing = table.Column<bool>(type: "boolean", nullable: false),
                    IsGeneralTask = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeadAgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoordinatingAgencyIds = table.Column<string>(type: "jsonb", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluationType = table.Column<string>(type: "text", nullable: false),
                    CalculationMethod = table.Column<string>(type: "text", nullable: false),
                    CustomBaseline = table.Column<string>(type: "jsonb", nullable: false),
                    Deliverables = table.Column<string>(type: "jsonb", nullable: false),
                    AgencyDeliverables = table.Column<string>(type: "jsonb", nullable: false),
                    DynamicKPIs = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalTaskItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalTaskItems_Agencies_AssignedAgencyId",
                        column: x => x.AssignedAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoalTaskItems_Agencies_LeadAgencyId",
                        column: x => x.LeadAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoalTaskItems_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalTaskItems_GoalTaskItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalTaskItems_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    LinkUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgencyTaskExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculatedStatus = table.Column<string>(type: "text", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    LatestProgressValue = table.Column<decimal>(type: "numeric", nullable: true),
                    LatestQualitativeStatus = table.Column<string>(type: "text", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    Deliverables = table.Column<string>(type: "jsonb", nullable: false),
                    SummaryNotes = table.Column<string>(type: "text", nullable: true),
                    AttachmentFileUrls = table.Column<string>(type: "jsonb", nullable: false),
                    LastReportedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastReportedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyTaskExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyTaskExecutions_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyTaskExecutions_Agencies_AssignedAgencyId",
                        column: x => x.AssignedAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgencyTaskExecutions_GoalTaskItems_GoalTaskId",
                        column: x => x.GoalTaskId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodQuarter = table.Column<int>(type: "integer", nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    QuantitativeValue = table.Column<decimal>(type: "numeric", nullable: true),
                    QualitativeStatus = table.Column<string>(type: "text", nullable: true),
                    SummaryNotes = table.Column<string>(type: "text", nullable: false),
                    CalculatedProgressPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    AttachmentFileUrls = table.Column<string>(type: "jsonb", nullable: false),
                    Deliverables = table.Column<string>(type: "jsonb", nullable: true),
                    CalculatedAlert = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressLogs_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgressLogs_GoalTaskItems_GoalTaskId",
                        column: x => x.GoalTaskId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetBaselines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Quarter = table.Column<int>(type: "integer", nullable: false),
                    TargetQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    TargetQualitativeStatus = table.Column<string>(type: "text", nullable: true),
                    IsCustomOverride = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideNote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetBaselines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetBaselines_GoalTaskItems_GoalTaskId",
                        column: x => x.GoalTaskId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskUrgeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalTaskItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeadAgencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaskCode = table.Column<string>(type: "text", nullable: false),
                    TaskTitle = table.Column<string>(type: "text", nullable: false),
                    UrgeContent = table.Column<string>(type: "text", nullable: false),
                    ForecastDataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    RecipientsSummary = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskUrgeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskUrgeLogs_Agencies_LeadAgencyId",
                        column: x => x.LeadAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskUrgeLogs_GoalTaskItems_GoalTaskItemId",
                        column: x => x.GoalTaskItemId,
                        principalTable: "GoalTaskItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_Code",
                table: "Agencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_ParentId",
                table: "Agencies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyTaskExecutions_AgencyId",
                table: "AgencyTaskExecutions",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyTaskExecutions_AssignedAgencyId",
                table: "AgencyTaskExecutions",
                column: "AssignedAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyTaskExecutions_GoalTaskId_AgencyId",
                table: "AgencyTaskExecutions",
                columns: new[] { "GoalTaskId", "AgencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentNumber",
                table: "Documents",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_AssignedAgencyId",
                table: "GoalTaskItems",
                column: "AssignedAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_DocumentId",
                table: "GoalTaskItems",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_LeadAgencyId",
                table: "GoalTaskItems",
                column: "LeadAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_ParentId",
                table: "GoalTaskItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_UnitId",
                table: "GoalTaskItems",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AgencyId",
                table: "Notifications",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressLogs_AgencyId",
                table: "ProgressLogs",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressLogs_GoalTaskId",
                table: "ProgressLogs",
                column: "GoalTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetBaselines_GoalTaskId_Year_Quarter",
                table: "TargetBaselines",
                columns: new[] { "GoalTaskId", "Year", "Quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskUrgeLogs_GoalTaskItemId",
                table: "TaskUrgeLogs",
                column: "GoalTaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUrgeLogs_LeadAgencyId",
                table: "TaskUrgeLogs",
                column: "LeadAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Code",
                table: "Units",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgencyId",
                table: "Users",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyTaskExecutions");

            migrationBuilder.DropTable(
                name: "DataImportLogs");

            migrationBuilder.DropTable(
                name: "LegalDocuments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProgressLogs");

            migrationBuilder.DropTable(
                name: "TargetBaselines");

            migrationBuilder.DropTable(
                name: "TaskUrgeLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GoalTaskItems");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Units");
        }
    }
}
