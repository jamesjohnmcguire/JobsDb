-- Current JobsDb SQLite schema, generated from EF migrations.
-- Load into a new empty database; use migrations to upgrade existing data.

BEGIN TRANSACTION;
CREATE TABLE "Credentials" (
	"id" INTEGER NOT NULL CONSTRAINT "PK_Credentials" PRIMARY KEY AUTOINCREMENT,
	"source" TEXT NOT NULL,
	"username" TEXT NOT NULL,
	"encryptedPassword" TEXT NOT NULL,
	"cookieData" TEXT NOT NULL,
	"lastUsed" TEXT NULL,
	"isActive" INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE "Jobs" (
	"id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
	"title" TEXT NOT NULL,
	"company" TEXT NOT NULL,
	"location" TEXT NOT NULL,
	"description" TEXT NOT NULL,
	"requirements" TEXT NOT NULL,
	"source" TEXT NOT NULL,
	"sourceUrl" TEXT NOT NULL,
	"sourceJobId" TEXT NOT NULL,
	"salaryMin" TEXT NULL,
	"salaryMax" TEXT NULL,
	"salaryCurrency" TEXT NOT NULL,
	"jobType" TEXT NOT NULL,
	"remoteType" TEXT NOT NULL,
	"datePosted" TEXT NOT NULL,
	"dateScraped" TEXT NOT NULL DEFAULT (datetime('now')),
	"dateApplied" TEXT NULL,
	"status" TEXT NOT NULL,
	"priority" INTEGER NULL,
	"notes" TEXT NOT NULL,
	"isArchived" INTEGER NOT NULL
);

CREATE TABLE "ScraperLogs" (
	"id" INTEGER NOT NULL CONSTRAINT "PK_ScraperLogs" PRIMARY KEY AUTOINCREMENT,
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
	"id" INTEGER NOT NULL
		CONSTRAINT "PK_SearchFilters" PRIMARY KEY AUTOINCREMENT,
	"name" TEXT NOT NULL,
	"keywords" TEXT NOT NULL,
	"location" TEXT NOT NULL,
	"source" TEXT NOT NULL,
	"isActive" INTEGER NOT NULL DEFAULT 1,
	"createdDate" TEXT NOT NULL DEFAULT (datetime('now')),
	"lastRunDate" TEXT NULL
);

CREATE TABLE "__EFMigrationsHistory" (
	"MigrationId" TEXT NOT NULL
		CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260723233159_InitialCreate', '10.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116204352_UseCamelCaseColumns', '10.0.11');

COMMIT;
