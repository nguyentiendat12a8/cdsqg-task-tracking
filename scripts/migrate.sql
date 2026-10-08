CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "Agencies" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" character varying(250) NOT NULL,
        "Type" text NOT NULL,
        "ParentId" uuid,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp without time zone NOT NULL,
        "ContactPersons" text NOT NULL,
        "PlanFiles" text NOT NULL,
        CONSTRAINT "PK_Agencies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Agencies_Agencies_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Agencies" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "DataImportLogs" (
        "Id" uuid NOT NULL,
        "FileName" text NOT NULL,
        "FileType" text NOT NULL,
        "ImportedAt" timestamp without time zone NOT NULL,
        "ImportedBy" text NOT NULL,
        "TotalGoalsCreated" integer NOT NULL,
        "TotalTasksCreated" integer NOT NULL,
        "Status" text NOT NULL,
        "SummaryNotes" text NOT NULL,
        "AgencyId" uuid,
        CONSTRAINT "PK_DataImportLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "Documents" (
        "Id" uuid NOT NULL,
        "DocumentNumber" text NOT NULL,
        "Name" text NOT NULL,
        "Summary" text,
        "Signer" text,
        "IssueDate" timestamp without time zone NOT NULL,
        "TimeResolution" text NOT NULL,
        "StartYear" integer,
        "EndYear" integer,
        "AttachmentPath" text,
        "AttachmentFilePaths" jsonb NOT NULL,
        "CreatedAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_Documents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "LegalDocuments" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Title" text NOT NULL,
        "DocumentType" text NOT NULL,
        "IssuingAgencyId" uuid,
        "IssuingAgencyName" text,
        "DraftingAgencyId" uuid,
        "DraftingAgencyName" text,
        "SignerName" text,
        "SignerTitle" text,
        "IssuedDate" timestamp without time zone,
        "EffectiveDate" timestamp without time zone,
        "EffectStatus" text NOT NULL,
        "Field" text,
        "Scope" text,
        "AttachmentsJson" text,
        "Notes" text,
        "CreatedByAgencyId" uuid,
        "CreatedByUserId" uuid,
        "CreatedAt" timestamp without time zone NOT NULL,
        "UpdatedAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_LegalDocuments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "Units" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "DataType" text NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Units" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Username" text NOT NULL,
        "PasswordHash" text NOT NULL,
        "FullName" text NOT NULL,
        "Email" text NOT NULL,
        "Role" text NOT NULL,
        "AgencyId" uuid,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp without time zone NOT NULL,
        "LastLoginAt" timestamp without time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Users_Agencies_AgencyId" FOREIGN KEY ("AgencyId") REFERENCES "Agencies" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "GoalTaskItems" (
        "Id" uuid NOT NULL,
        "DocumentId" uuid NOT NULL,
        "ItemType" text NOT NULL,
        "ParentId" uuid,
        "Code" text NOT NULL,
        "Title" text NOT NULL,
        "Category" text NOT NULL,
        "Section" text,
        "Group" text,
        "IsOngoing" boolean NOT NULL,
        "IsGeneralTask" boolean NOT NULL,
        "StartDate" timestamp without time zone,
        "DueDate" timestamp without time zone,
        "LeadAgencyId" uuid NOT NULL,
        "AssignedAgencyId" uuid,
        "CoordinatingAgencyIds" jsonb NOT NULL,
        "UnitId" uuid,
        "EvaluationType" text NOT NULL,
        "CalculationMethod" text NOT NULL,
        "CustomBaseline" jsonb NOT NULL,
        "Deliverables" jsonb NOT NULL,
        "AgencyDeliverables" jsonb NOT NULL,
        "DynamicKPIs" jsonb NOT NULL,
        "CreatedAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_GoalTaskItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GoalTaskItems_Agencies_AssignedAgencyId" FOREIGN KEY ("AssignedAgencyId") REFERENCES "Agencies" ("Id"),
        CONSTRAINT "FK_GoalTaskItems_Agencies_LeadAgencyId" FOREIGN KEY ("LeadAgencyId") REFERENCES "Agencies" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_GoalTaskItems_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_GoalTaskItems_GoalTaskItems_ParentId" FOREIGN KEY ("ParentId") REFERENCES "GoalTaskItems" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_GoalTaskItems_Units_UnitId" FOREIGN KEY ("UnitId") REFERENCES "Units" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "AgencyId" uuid,
        "Title" text NOT NULL,
        "Message" text NOT NULL,
        "Type" text NOT NULL,
        "IsRead" boolean NOT NULL,
        "LinkUrl" text,
        "CreatedAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Agencies_AgencyId" FOREIGN KEY ("AgencyId") REFERENCES "Agencies" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "AgencyTaskExecutions" (
        "Id" uuid NOT NULL,
        "GoalTaskId" uuid NOT NULL,
        "AgencyId" uuid NOT NULL,
        "AssignedAgencyId" uuid,
        "CalculatedStatus" text NOT NULL,
        "ApprovalStatus" integer NOT NULL,
        "RejectionReason" text,
        "LatestProgressValue" numeric,
        "LatestQualitativeStatus" text,
        "CompletionPercentage" numeric NOT NULL,
        "Deliverables" jsonb NOT NULL,
        "SummaryNotes" text,
        "AttachmentFileUrls" jsonb NOT NULL,
        "LastReportedAt" timestamp without time zone,
        "LastReportedBy" text,
        "CreatedAt" timestamp without time zone NOT NULL,
        "UpdatedAt" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_AgencyTaskExecutions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AgencyTaskExecutions_Agencies_AgencyId" FOREIGN KEY ("AgencyId") REFERENCES "Agencies" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AgencyTaskExecutions_Agencies_AssignedAgencyId" FOREIGN KEY ("AssignedAgencyId") REFERENCES "Agencies" ("Id"),
        CONSTRAINT "FK_AgencyTaskExecutions_GoalTaskItems_GoalTaskId" FOREIGN KEY ("GoalTaskId") REFERENCES "GoalTaskItems" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "ProgressLogs" (
        "Id" uuid NOT NULL,
        "GoalTaskId" uuid NOT NULL,
        "PeriodYear" integer NOT NULL,
        "PeriodQuarter" integer NOT NULL,
        "LogDate" timestamp without time zone NOT NULL,
        "QuantitativeValue" numeric,
        "QualitativeStatus" text,
        "SummaryNotes" text NOT NULL,
        "CalculatedProgressPercentage" numeric,
        "AttachmentFileUrls" jsonb NOT NULL,
        "Deliverables" jsonb,
        "CalculatedAlert" text NOT NULL,
        "CreatedBy" text NOT NULL,
        "AgencyId" uuid,
        "ApprovalStatus" integer NOT NULL,
        "RejectionReason" text,
        "ApprovedBy" text,
        "ApprovedAt" timestamp without time zone,
        CONSTRAINT "PK_ProgressLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProgressLogs_Agencies_AgencyId" FOREIGN KEY ("AgencyId") REFERENCES "Agencies" ("Id"),
        CONSTRAINT "FK_ProgressLogs_GoalTaskItems_GoalTaskId" FOREIGN KEY ("GoalTaskId") REFERENCES "GoalTaskItems" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "TargetBaselines" (
        "Id" uuid NOT NULL,
        "GoalTaskId" uuid NOT NULL,
        "Year" integer NOT NULL,
        "Quarter" integer NOT NULL,
        "TargetQuantity" numeric,
        "TargetQualitativeStatus" text,
        "IsCustomOverride" boolean NOT NULL,
        "OverrideNote" text,
        CONSTRAINT "PK_TargetBaselines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TargetBaselines_GoalTaskItems_GoalTaskId" FOREIGN KEY ("GoalTaskId") REFERENCES "GoalTaskItems" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE TABLE "TaskUrgeLogs" (
        "Id" uuid NOT NULL,
        "GoalTaskId" uuid NOT NULL,
        "GoalTaskItemId" uuid,
        "LeadAgencyId" uuid,
        "TaskCode" text NOT NULL,
        "TaskTitle" text NOT NULL,
        "UrgeContent" text NOT NULL,
        "ForecastDataJson" text NOT NULL,
        "CreatedAt" timestamp without time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "RecipientsSummary" text,
        "Title" text,
        CONSTRAINT "PK_TaskUrgeLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaskUrgeLogs_Agencies_LeadAgencyId" FOREIGN KEY ("LeadAgencyId") REFERENCES "Agencies" ("Id"),
        CONSTRAINT "FK_TaskUrgeLogs_GoalTaskItems_GoalTaskItemId" FOREIGN KEY ("GoalTaskItemId") REFERENCES "GoalTaskItems" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_Agencies_Code" ON "Agencies" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_Agencies_ParentId" ON "Agencies" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_AgencyTaskExecutions_AgencyId" ON "AgencyTaskExecutions" ("AgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_AgencyTaskExecutions_AssignedAgencyId" ON "AgencyTaskExecutions" ("AssignedAgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_AgencyTaskExecutions_GoalTaskId_AgencyId" ON "AgencyTaskExecutions" ("GoalTaskId", "AgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_Documents_DocumentNumber" ON "Documents" ("DocumentNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_GoalTaskItems_AssignedAgencyId" ON "GoalTaskItems" ("AssignedAgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_GoalTaskItems_DocumentId" ON "GoalTaskItems" ("DocumentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_GoalTaskItems_LeadAgencyId" ON "GoalTaskItems" ("LeadAgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_GoalTaskItems_ParentId" ON "GoalTaskItems" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_GoalTaskItems_UnitId" ON "GoalTaskItems" ("UnitId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_Notifications_AgencyId" ON "Notifications" ("AgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_ProgressLogs_AgencyId" ON "ProgressLogs" ("AgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_ProgressLogs_GoalTaskId" ON "ProgressLogs" ("GoalTaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_TargetBaselines_GoalTaskId_Year_Quarter" ON "TargetBaselines" ("GoalTaskId", "Year", "Quarter");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_TaskUrgeLogs_GoalTaskItemId" ON "TaskUrgeLogs" ("GoalTaskItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_TaskUrgeLogs_LeadAgencyId" ON "TaskUrgeLogs" ("LeadAgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_Units_Code" ON "Units" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE INDEX "IX_Users_AgencyId" ON "Users" ("AgencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151316_InitialSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261008151316_InitialSchema', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151614_PasswordRecovery') THEN
    ALTER TABLE "Users" ADD "PasswordResetExpiresAt" timestamp without time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151614_PasswordRecovery') THEN
    ALTER TABLE "Users" ADD "PasswordResetTokenHash" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151614_PasswordRecovery') THEN
    ALTER TABLE "Users" ADD "SecurityStamp" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008151614_PasswordRecovery') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261008151614_PasswordRecovery', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    CREATE TEMP TABLE retired_child_items ON COMMIT DROP AS
        SELECT "Id" FROM "GoalTaskItems" WHERE "ParentId" IS NOT NULL;
    DELETE FROM "Notifications" n WHERE EXISTS
        (SELECT 1 FROM retired_child_items c WHERE n."LinkUrl" LIKE '%' || c."Id"::text || '%');
    DELETE FROM "TaskUrgeLogs" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items)
        OR "GoalTaskItemId" IN (SELECT "Id" FROM retired_child_items);
    DELETE FROM "AgencyTaskExecutions" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
    DELETE FROM "ProgressLogs" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
    DELETE FROM "TargetBaselines" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
    DELETE FROM "GoalTaskItems" WHERE "Id" IN (SELECT "Id" FROM retired_child_items);
    DELETE FROM "ProgressLogs" WHERE "PeriodQuarter" <> 0;
    DELETE FROM "TargetBaselines" WHERE "Quarter" <> 0;
    UPDATE "GoalTaskItems" i SET "CustomBaseline" = COALESCE(
        (SELECT jsonb_object_agg(key, value) FROM jsonb_each(i."CustomBaseline")
            WHERE key ~ '^[0-9]{4}$'), '{}'::jsonb), "AgencyDeliverables" = '{}'::jsonb;
    UPDATE "AgencyTaskExecutions" SET "CompletionPercentage" = 0,
        "LatestProgressValue" = NULL, "LatestQualitativeStatus" = NULL,
        "CalculatedStatus" = 'NotStarted', "Deliverables" = '[]'::jsonb,
        "SummaryNotes" = NULL, "AttachmentFileUrls" = '[]'::jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    ALTER TABLE "GoalTaskItems" DROP CONSTRAINT IF EXISTS "FK_GoalTaskItems_GoalTaskItems_ParentId";
    DROP INDEX IF EXISTS "IX_TargetBaselines_GoalTaskId_Year_Quarter";
    DROP INDEX IF EXISTS "IX_GoalTaskItems_ParentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    ALTER TABLE "TargetBaselines" DROP COLUMN "Quarter";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    ALTER TABLE "ProgressLogs" DROP COLUMN "PeriodQuarter";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    ALTER TABLE "GoalTaskItems" DROP COLUMN "ParentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    CREATE UNIQUE INDEX "IX_TargetBaselines_GoalTaskId_Year" ON "TargetBaselines" ("GoalTaskId", "Year");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008164528_AnnualReportingOnly') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261008164528_AnnualReportingOnly', '9.0.2');
    END IF;
END $EF$;
COMMIT;

