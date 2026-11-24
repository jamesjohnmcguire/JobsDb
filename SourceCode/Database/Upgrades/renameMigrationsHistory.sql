BEGIN IMMEDIATE;

ALTER TABLE "__EFMigrationsHistory" RENAME TO "MigrationsHistory";
INSERT INTO "MigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261123220231_RenameMigrationsHistory', '11.0.0');

COMMIT;
