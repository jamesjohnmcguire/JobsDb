-- Authoritative current JobsDb SQLite schema.
-- Load into a new empty database; use migrations to upgrade existing data.

BEGIN TRANSACTION;
CREATE TABLE "Credentials" (
	"id" INTEGER PRIMARY KEY AUTOINCREMENT,
	"source" TEXT NOT NULL,
	"username" TEXT NOT NULL,
	"encryptedPassword" TEXT NOT NULL,
	"cookieData" TEXT NOT NULL,
	"lastUsed" TEXT NULL,
	"isActive" INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE "Jobs" (
	"id" INTEGER PRIMARY KEY AUTOINCREMENT,
	"company" TEXT NOT NULL,
	"dateApplied" TEXT NULL,
	"datePosted" TEXT NOT NULL,
	"dateScraped" TEXT NOT NULL DEFAULT (datetime('now')),
	"description" TEXT NULL,
	"isArchived" INTEGER NOT NULL,
	"jobType" TEXT NULL,
	"location" TEXT NULL,
	"notes" TEXT NULL,
	"priority" INTEGER NULL,
	"remoteType" TEXT NULL,
	"requirements" TEXT NULL,
	"salaryCurrency" TEXT NULL,
	"salaryMax" TEXT NULL,
	"salaryMin" TEXT NULL,
	"source" TEXT NOT NULL,
	"sourceJobId" TEXT NOT NULL,
	"sourceUrl" TEXT NOT NULL,
	"status" TEXT NOT NULL,
	"title" TEXT NOT NULL
);

CREATE TABLE "ScraperLogs" (
	"id" INTEGER PRIMARY KEY AUTOINCREMENT,
	"source" TEXT NOT NULL,
	"timestamp" TEXT NOT NULL DEFAULT (datetime('now')),
	"jobsFound" INTEGER NOT NULL,
	"jobsAdded" INTEGER NOT NULL,
	"jobsUpdated" INTEGER NOT NULL,
	"success" INTEGER NOT NULL,
	"errorMessage" TEXT NOT NULL,
	"durationMs" INTEGER NOT NULL
);

CREATE TABLE "SearchFilters" (
	"id" INTEGER PRIMARY KEY AUTOINCREMENT,
	"name" TEXT NOT NULL,
	"keywords" TEXT NOT NULL,
	"location" TEXT NOT NULL,
	"source" TEXT NOT NULL,
	"isActive" INTEGER NOT NULL DEFAULT 1,
	"createdDate" TEXT NOT NULL DEFAULT (datetime('now')),
	"lastRunDate" TEXT NULL
);

CREATE TABLE "MigrationsHistory" (
	"MigrationId" TEXT NOT NULL
		CONSTRAINT "PK_MigrationsHistory" PRIMARY KEY,
	"ProductVersion" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_Credentials_source" ON "Credentials" ("source");

CREATE INDEX "IX_Jobs_company" ON "Jobs" ("company");

CREATE INDEX "IX_Jobs_datePosted" ON "Jobs" ("datePosted");

CREATE INDEX "IX_Jobs_isArchived" ON "Jobs" ("isArchived");

CREATE INDEX "IX_Jobs_location" ON "Jobs" ("location");

CREATE INDEX "IX_Jobs_sourceJobId" ON "Jobs" ("sourceJobId");

CREATE UNIQUE INDEX "IX_Jobs_source_sourceJobId"
	ON "Jobs" ("source", "sourceJobId");

CREATE INDEX "IX_Jobs_status" ON "Jobs" ("status");

CREATE INDEX "IX_ScraperLogs_source" ON "ScraperLogs" ("source");

CREATE INDEX "IX_ScraperLogs_success" ON "ScraperLogs" ("success");

CREATE INDEX "IX_ScraperLogs_timestamp" ON "ScraperLogs" ("timestamp");

CREATE INDEX "IX_SearchFilters_isActive" ON "SearchFilters" ("isActive");

CREATE INDEX "IX_SearchFilters_name" ON "SearchFilters" ("name");

INSERT INTO "MigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260723233159_InitialCreate', '10.0.11');

INSERT INTO "MigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116204352_UseCamelCaseColumns', '10.0.11');

INSERT INTO "MigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116222114_AllowOptionalJobDetails', '10.0.11');

INSERT INTO "MigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261123220231_RenameMigrationsHistory', '11.0.0');

COMMIT;
