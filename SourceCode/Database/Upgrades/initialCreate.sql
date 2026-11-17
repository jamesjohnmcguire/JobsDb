BEGIN IMMEDIATE;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
	"MigrationId" TEXT NOT NULL
		CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
	"ProductVersion" TEXT NOT NULL
);

CREATE TABLE "Credentials" (
	"Id" INTEGER NOT NULL CONSTRAINT "PK_Credentials" PRIMARY KEY AUTOINCREMENT,
	"Source" TEXT NOT NULL,
	"Username" TEXT NOT NULL,
	"EncryptedPassword" TEXT NOT NULL,
	"CookieData" TEXT NOT NULL,
	"LastUsed" TEXT NULL,
	"IsActive" INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE "Jobs" (
	"Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
	"Title" TEXT NOT NULL,
	"Company" TEXT NOT NULL,
	"Location" TEXT NOT NULL,
	"Description" TEXT NOT NULL,
	"Requirements" TEXT NOT NULL,
	"Source" TEXT NOT NULL,
	"SourceUrl" TEXT NOT NULL,
	"SourceJobId" TEXT NOT NULL,
	"SalaryMin" TEXT NULL,
	"SalaryMax" TEXT NULL,
	"SalaryCurrency" TEXT NOT NULL,
	"JobType" TEXT NOT NULL,
	"RemoteType" TEXT NOT NULL,
	"DatePosted" TEXT NOT NULL,
	"DateScraped" TEXT NOT NULL DEFAULT (datetime('now')),
	"DateApplied" TEXT NULL,
	"Status" TEXT NOT NULL,
	"Priority" INTEGER NULL,
	"Notes" TEXT NOT NULL,
	"IsArchived" INTEGER NOT NULL
);

CREATE TABLE "ScraperLogs" (
	"Id" INTEGER NOT NULL CONSTRAINT "PK_ScraperLogs" PRIMARY KEY AUTOINCREMENT,
	"Source" TEXT NOT NULL,
	"Timestamp" TEXT NOT NULL DEFAULT (datetime('now')),
	"JobsFound" INTEGER NOT NULL,
	"JobsAdded" INTEGER NOT NULL,
	"JobsUpdated" INTEGER NOT NULL,
	"Success" INTEGER NOT NULL,
	"ErrorMessage" TEXT NOT NULL,
	"DurationMs" INTEGER NOT NULL
);

CREATE TABLE "SearchFilters" (
	"Id" INTEGER NOT NULL
		CONSTRAINT "PK_SearchFilters" PRIMARY KEY AUTOINCREMENT,
	"Name" TEXT NOT NULL,
	"Keywords" TEXT NOT NULL,
	"Location" TEXT NOT NULL,
	"Source" TEXT NOT NULL,
	"IsActive" INTEGER NOT NULL DEFAULT 1,
	"CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
	"LastRunDate" TEXT NULL
);

CREATE UNIQUE INDEX "IX_Credentials_Source" ON "Credentials" ("Source");

CREATE INDEX "IX_Jobs_Company" ON "Jobs" ("Company");

CREATE INDEX "IX_Jobs_DatePosted" ON "Jobs" ("DatePosted");

CREATE INDEX "IX_Jobs_IsArchived" ON "Jobs" ("IsArchived");

CREATE INDEX "IX_Jobs_Location" ON "Jobs" ("Location");

CREATE UNIQUE INDEX "IX_Jobs_Source_SourceJobId"
	ON "Jobs" ("Source", "SourceJobId");

CREATE INDEX "IX_Jobs_SourceJobId" ON "Jobs" ("SourceJobId");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_ScraperLogs_Source" ON "ScraperLogs" ("Source");

CREATE INDEX "IX_ScraperLogs_Success" ON "ScraperLogs" ("Success");

CREATE INDEX "IX_ScraperLogs_Timestamp" ON "ScraperLogs" ("Timestamp");

CREATE INDEX "IX_SearchFilters_IsActive" ON "SearchFilters" ("IsActive");

CREATE INDEX "IX_SearchFilters_Name" ON "SearchFilters" ("Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260723233159_InitialCreate', '10.0.11');

COMMIT;
