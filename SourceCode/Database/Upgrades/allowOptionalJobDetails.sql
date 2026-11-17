PRAGMA foreign_keys = OFF;

BEGIN IMMEDIATE;

CREATE TABLE "JobsUpgrade" (
	"id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
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

INSERT INTO "JobsUpgrade" (
	"id",
	"company",
	"dateApplied",
	"datePosted",
	"dateScraped",
	"description",
	"isArchived",
	"jobType",
	"location",
	"notes",
	"priority",
	"remoteType",
	"requirements",
	"salaryCurrency",
	"salaryMax",
	"salaryMin",
	"source",
	"sourceJobId",
	"sourceUrl",
	"status",
	"title"
)
SELECT
	"id",
	"company",
	"dateApplied",
	"datePosted",
	"dateScraped",
	"description",
	"isArchived",
	"jobType",
	"location",
	"notes",
	"priority",
	"remoteType",
	"requirements",
	"salaryCurrency",
	"salaryMax",
	"salaryMin",
	"source",
	"sourceJobId",
	"sourceUrl",
	"status",
	"title"
FROM "Jobs";

-- Preserve identifiers consumed by postings that have already been deleted.
DELETE FROM "sqlite_sequence" WHERE "name" = 'JobsUpgrade';
INSERT INTO "sqlite_sequence" ("name", "seq")
SELECT 'JobsUpgrade', "seq"
FROM "sqlite_sequence"
WHERE "name" = 'Jobs';

DROP TABLE "Jobs";

ALTER TABLE "JobsUpgrade" RENAME TO "Jobs";

CREATE INDEX "IX_Jobs_company" ON "Jobs" ("company");

CREATE INDEX "IX_Jobs_datePosted" ON "Jobs" ("datePosted");

CREATE INDEX "IX_Jobs_isArchived" ON "Jobs" ("isArchived");

CREATE INDEX "IX_Jobs_location" ON "Jobs" ("location");

CREATE UNIQUE INDEX "IX_Jobs_source_sourceJobId"
	ON "Jobs" ("source", "sourceJobId");

CREATE INDEX "IX_Jobs_sourceJobId" ON "Jobs" ("sourceJobId");

CREATE INDEX "IX_Jobs_status" ON "Jobs" ("status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116222114_AllowOptionalJobDetails', '10.0.11');

COMMIT;

PRAGMA foreign_keys = ON;
