# SQL-First Database Workflow

## Source of Truth

`SourceCode/Database/schema.sql` is the authoritative desired schema. Edit it directly; tooling must never overwrite it from EF models or a runtime database. Use PascalCase table names, camelCase column names, and the preferred `"id" INTEGER PRIMARY KEY AUTOINCREMENT` declaration. Reference data belongs in `staticData.sql` only when required.

EF Core can remain the application data-access layer. Migrations describe the steps from an older database to the schema declared in SQL; they are upgrade history, not the authority for the current design.

## Architecture Boundary

Follow the [layered architecture](Architecture.md). EF and generated persistence code stay inside infrastructure. Schema preparation must preserve handwritten domain rules and enum semantics; generating database classes must not turn them into UI or business models. The isolated scaffolding experiment must establish this boundary before the build adopts generation.

## Current Implementation

The existing application initialization and `updateDatabase.*` scripts still apply EF migrations. The existing `schema.sql` contains the equivalent current schema and EF history baseline. The transition below is planned, not yet implemented. Do not edit SQL and expect today's updater to detect or apply that edit.

The proposed exporter has been removed. SQLite's `.schema` command is sufficient for inspecting a database, and `.read` loads SQL into a new database. Neither command infers a data-preserving upgrade from two schemas.

## Mapping Experiment Status

The [isolated mapping experiment](sqlFirstMappingReview.md) identified storage-type differences. A Jobs-only generated context and `JobStorageMapper` now prove compatibility with the existing repository in both directions, including optional values, using EF's built-in conversions. This is an infrastructure proof; the application still uses `JobsDbContext`. Extend the approach to the remaining tables and add repeatable schema preparation before adopting generation in the build. The updater explicitly selects `JobsDbContext` while both contexts exist.

## Atomic Implementation Steps

1. **Prove SQL-to-EF generation in isolation.** Create a temporary database using `sqlite3 -bail` and `.read schema.sql`, then use `dotnet ef dbcontext scaffold` to inspect generated mappings. Exclude migration-history tables. Establish stable entity names and preserve existing enum conversions, optional fields, and application behavior. Keep generated mappings separate from handwritten behavior. Acceptance: the current repositories can use the SQL-derived mappings without behavioral changes. Do not overwrite the current context or models as an experiment.
2. **Add lean schema preparation scripts.** Add camelCase Windows and Bash entry points that recreate only a designated disposable schema database, generate the agreed EF files, and validate the result. Never target the application database for this stage. Acceptance: a supported SQL column change reaches its EF mapping with no duplicate handwritten schema edit; incompatible application references fail clearly during build.
3. **Retain reviewed upgrade migrations.** Generate an EF migration from the SQL-derived model when a schema change is ready. Review renames, table rebuilds, conversions, and required-column backfills. Commit SQL, generated mappings, and the corresponding migration together. Ordinary builds must not create new migrations automatically. Acceptance: upgrading a database from the preceding version produces the schema declared in SQL and preserves records, notes, and application status.
4. **Connect the build stages.** Prepare and validate the SQL-derived model, rebuild, apply reviewed upgrades through `updateDatabase.*`, then run tests. Keep a destructive `rebuildDatabase.*` operation separate and explicit. Acceptance: SQL is never regenerated; unsupported changes stop the workflow before updating application data.
5. **Finish schema cleanup separately.** Apply the concise primary-key declarations and history-table naming changes as individual reviewed steps. Compare semantic schema metadata rather than exact SQL formatting. Move EF baseline bookkeeping out of the hand-maintained application schema if the chosen bootstrap design permits it.

## Limits and Verification

Scaffolding derives database mappings, not business intent. SQLite column types cannot fully encode C# enums or application validation. Custom mappings may still be needed. Renaming a column and replacing a column with a different one can have the same final schema but require different upgrade steps. These changes require an explicit migration decision.

Tests must treat the SQL-created database as the expected schema, compare migrated databases against it, and exercise real repository reads and writes. Test both fresh creation and upgrades. Keep scraper redesign deferred; dashboard connectivity follows the database foundation.
