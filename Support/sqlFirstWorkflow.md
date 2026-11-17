# SQL-First Database Workflow

## Source of Truth

`SourceCode/Database/schema.sql` is the authoritative desired schema. Edit it directly; tooling must not overwrite it from C# models or a runtime database. Tables use PascalCase, columns use camelCase, and SQLite integer keys use `"id" INTEGER PRIMARY KEY AUTOINCREMENT`.

Versioned SQL upgrade scripts describe how existing databases reach that schema while preserving data. `staticData.sql` is only for required reference data; updates must be repeatable without duplicating records or replacing user configuration.

## Architecture Decision

Database preparation runs independently of application compilation. The target order is SQL upgrades, required data, database validation, data-access preparation, application build, then tests. A current database needs no upgrade and remains the basis for the downstream workflow.

EF Core may remain inside the [data-access layer](Architecture.md) as the ORM. Business logic uses typed models and interfaces; EF configuration, queries, and tracking stay inside data access. Compiled EF migrations will no longer be the normal update mechanism. Code generation remains optional and must justify its maintenance cost.

## Current Implementation

`rebuildDatabase.*` already loads the master SQL and optional static data into a temporary replacement before replacing an explicitly named development database. The `jobsdb.db` filename is reserved for application data. Rebuild remains separate from the normal build.

Native updaters now use SQLite directly: initialize an empty database from the master SQL, apply unrecorded upgrades, load optional repeatable static data, then check integrity and foreign keys. No application compilation is required. Existing EF history and version IDs remain compatible. Library readiness validation now checks the SQL manifest and mapped queries without creating or upgrading databases. Startup invokes it once and owns context disposal.

Reviewed SQL history is now checked in under `SourceCode/Database/Upgrades/`. `upgradeOrder.txt` pairs each existing migration ID with its camelCase SQL filename, preserving the installed history baseline. Each upgrade commits its version with its changes. The Jobs rebuild also preserves identifiers consumed by deleted postings. The native updaters now consume these files.

The Jobs-only generated context and mapper remain an isolated compatibility proof. Do not expand the duplicate storage model by default.

## Atomic Implementation Steps

1. Establish ordered SQL upgrades and applied-version tracking compatible with existing databases. Review renames, table rebuilds, conversions, and required-value backfills explicitly.
2. Completed: remove automatic EF startup migrations, validate readiness in the library, and keep context lifetime explicit. A focused library initialization entry point should hide the remaining EF construction and service wiring from thin clients.
3. Validate upgraded databases against the master schema and verify data preservation. The normal build must finish this database phase before compiling the application and running integration or end-to-end tests.
4. Extract the data-access boundary incrementally, deciding shared model and interface ownership first. Preserve existing typed behavior and repository contracts.
5. Apply the concise primary-key declarations and history-table naming cleanup in separate atomic changes. Compare schema semantics rather than generated SQL formatting.

## Limits

Editing a final CREATE TABLE definition cannot express every data-preserving transition. Upgrade SQL may be drafted with tooling, but rename intent and data transformations need review. Schema changes can require mapping or application changes; EF conventions must not dictate those changes. Keep scraper redesign deferred while the database foundation is completed.
