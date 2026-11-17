BEGIN IMMEDIATE;

ALTER TABLE "SearchFilters" RENAME COLUMN "Source" TO "source";

ALTER TABLE "SearchFilters" RENAME COLUMN "Name" TO "name";

ALTER TABLE "SearchFilters" RENAME COLUMN "Location" TO "location";

ALTER TABLE "SearchFilters" RENAME COLUMN "LastRunDate" TO "lastRunDate";

ALTER TABLE "SearchFilters" RENAME COLUMN "Keywords" TO "keywords";

ALTER TABLE "SearchFilters" RENAME COLUMN "IsActive" TO "isActive";

ALTER TABLE "SearchFilters" RENAME COLUMN "CreatedDate" TO "createdDate";

ALTER TABLE "SearchFilters" RENAME COLUMN "Id" TO "id";

DROP INDEX "IX_SearchFilters_Name";

CREATE INDEX "IX_SearchFilters_name" ON "SearchFilters" ("name");

DROP INDEX "IX_SearchFilters_IsActive";

CREATE INDEX "IX_SearchFilters_isActive" ON "SearchFilters" ("isActive");

ALTER TABLE "ScraperLogs" RENAME COLUMN "Timestamp" TO "timestamp";

ALTER TABLE "ScraperLogs" RENAME COLUMN "Success" TO "success";

ALTER TABLE "ScraperLogs" RENAME COLUMN "Source" TO "source";

ALTER TABLE "ScraperLogs" RENAME COLUMN "JobsUpdated" TO "jobsUpdated";

ALTER TABLE "ScraperLogs" RENAME COLUMN "JobsFound" TO "jobsFound";

ALTER TABLE "ScraperLogs" RENAME COLUMN "JobsAdded" TO "jobsAdded";

ALTER TABLE "ScraperLogs" RENAME COLUMN "ErrorMessage" TO "errorMessage";

ALTER TABLE "ScraperLogs" RENAME COLUMN "DurationMs" TO "durationMs";

ALTER TABLE "ScraperLogs" RENAME COLUMN "Id" TO "id";

DROP INDEX "IX_ScraperLogs_Timestamp";

CREATE INDEX "IX_ScraperLogs_timestamp" ON "ScraperLogs" ("timestamp");

DROP INDEX "IX_ScraperLogs_Success";

CREATE INDEX "IX_ScraperLogs_success" ON "ScraperLogs" ("success");

DROP INDEX "IX_ScraperLogs_Source";

CREATE INDEX "IX_ScraperLogs_source" ON "ScraperLogs" ("source");

ALTER TABLE "Jobs" RENAME COLUMN "Title" TO "title";

ALTER TABLE "Jobs" RENAME COLUMN "Status" TO "status";

ALTER TABLE "Jobs" RENAME COLUMN "SourceUrl" TO "sourceUrl";

ALTER TABLE "Jobs" RENAME COLUMN "SourceJobId" TO "sourceJobId";

ALTER TABLE "Jobs" RENAME COLUMN "Source" TO "source";

ALTER TABLE "Jobs" RENAME COLUMN "SalaryMin" TO "salaryMin";

ALTER TABLE "Jobs" RENAME COLUMN "SalaryMax" TO "salaryMax";

ALTER TABLE "Jobs" RENAME COLUMN "SalaryCurrency" TO "salaryCurrency";

ALTER TABLE "Jobs" RENAME COLUMN "Requirements" TO "requirements";

ALTER TABLE "Jobs" RENAME COLUMN "RemoteType" TO "remoteType";

ALTER TABLE "Jobs" RENAME COLUMN "Priority" TO "priority";

ALTER TABLE "Jobs" RENAME COLUMN "Notes" TO "notes";

ALTER TABLE "Jobs" RENAME COLUMN "Location" TO "location";

ALTER TABLE "Jobs" RENAME COLUMN "JobType" TO "jobType";

ALTER TABLE "Jobs" RENAME COLUMN "IsArchived" TO "isArchived";

ALTER TABLE "Jobs" RENAME COLUMN "Description" TO "description";

ALTER TABLE "Jobs" RENAME COLUMN "DateScraped" TO "dateScraped";

ALTER TABLE "Jobs" RENAME COLUMN "DatePosted" TO "datePosted";

ALTER TABLE "Jobs" RENAME COLUMN "DateApplied" TO "dateApplied";

ALTER TABLE "Jobs" RENAME COLUMN "Company" TO "company";

ALTER TABLE "Jobs" RENAME COLUMN "Id" TO "id";

DROP INDEX "IX_Jobs_Status";

CREATE INDEX "IX_Jobs_status" ON "Jobs" ("status");

DROP INDEX "IX_Jobs_SourceJobId";

CREATE INDEX "IX_Jobs_sourceJobId" ON "Jobs" ("sourceJobId");

DROP INDEX "IX_Jobs_Source_SourceJobId";

CREATE UNIQUE INDEX "IX_Jobs_source_sourceJobId"
	ON "Jobs" ("source", "sourceJobId");

DROP INDEX "IX_Jobs_Location";

CREATE INDEX "IX_Jobs_location" ON "Jobs" ("location");

DROP INDEX "IX_Jobs_IsArchived";

CREATE INDEX "IX_Jobs_isArchived" ON "Jobs" ("isArchived");

DROP INDEX "IX_Jobs_DatePosted";

CREATE INDEX "IX_Jobs_datePosted" ON "Jobs" ("datePosted");

DROP INDEX "IX_Jobs_Company";

CREATE INDEX "IX_Jobs_company" ON "Jobs" ("company");

ALTER TABLE "Credentials" RENAME COLUMN "Username" TO "username";

ALTER TABLE "Credentials" RENAME COLUMN "Source" TO "source";

ALTER TABLE "Credentials" RENAME COLUMN "LastUsed" TO "lastUsed";

ALTER TABLE "Credentials" RENAME COLUMN "IsActive" TO "isActive";

ALTER TABLE "Credentials" RENAME COLUMN "EncryptedPassword"
	TO "encryptedPassword";

ALTER TABLE "Credentials" RENAME COLUMN "CookieData" TO "cookieData";

ALTER TABLE "Credentials" RENAME COLUMN "Id" TO "id";

DROP INDEX "IX_Credentials_Source";

CREATE UNIQUE INDEX "IX_Credentials_source" ON "Credentials" ("source");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116204352_UseCamelCaseColumns', '10.0.11');

COMMIT;
