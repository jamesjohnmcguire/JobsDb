# Database Schema

`schema.sql` is the authoritative current SQLite schema: tables, columns, defaults, primary keys, and indexes. Edit this file directly; do not regenerate it from EF models. It also records the applied SQL upgrade baseline so databases created from SQL pass library readiness validation and accept subsequent SQL upgrades.

With `sqlite3` on PATH, rebuild a development database from the repository root:

```powershell
.\DevelopmentTools\rebuildDatabase.cmd review.db
```

For Bash, use `bash DevelopmentTools/rebuildDatabase.sh review.db`. The explicit destination is replaced, discarding its existing data; `jobsdb.db` is reserved for the application and refused in any directory. Close database connections first. Relative destinations resolve from your terminal's working directory, and the parent directory must exist. A temporary database is loaded before replacement, so SQL-loading failures leave the existing database intact. The normal build never calls rebuild.

Rebuild loads `schema.sql` and then `staticData.sql` if present. No required static-data file is currently needed. Scraped jobs, credentials, and optional sample filters are not installed. No EF generation or compilation is needed.

The updater applies reviewed SQL upgrades. Rebuild loads the master schema directly; upgrading an existing database requires a corresponding versioned SQL file. Neither command generates C# mappings. The [SQL-first transition plan](../../Support/sqlFirstWorkflow.md) defines how mappings and reviewed upgrade migrations will follow the SQL. The database-schema tests compare the SQL-created schema and migration history with a migrated database, then verify repeated application initialization preserves existing data.

Application tables use PascalCase and columns use camelCase. Maintain final table and index definitions here; keep historical upgrade steps in migrations.

Runtime and EF tooling share the default database path, `%LOCALAPPDATA%\JobsDb\jobsdb.db`. A path-based context resolves relative filenames against the current working directory and quotes connection-string characters safely. Supplied EF options and later connection overrides determine the reported data source; in-memory contexts report their configured memory source.

## SQL Upgrade History

The reviewed SQL files in `Upgrades/` follow `upgradeOrder.txt`, which lists the existing version ID and SQL filename on each line. A runner must skip recorded versions; each file writes its history entry in the same transaction as its schema changes. The updater follows this contract using SQLite directly. The SQL updater renames the legacy history table to MigrationsHistory and preserves existing version IDs. Run it before using EF tooling on an older database.

## Updating a Database

Run `DevelopmentTools/updateDatabase.cmd` or `bash DevelopmentTools/updateDatabase.sh`. An empty database is initialized from `schema.sql`; existing databases receive only unrecorded upgrades in `upgradeOrder.txt` order. Optional `staticData.sql` runs after upgrades and must use transactional, repeatable inserts or updates. The updater finishes with SQLite integrity and foreign-key checks. It requires `sqlite3`, not .NET or EF tooling.

The default is `%LOCALAPPDATA%\JobsDb\jobsdb.db`. To select another database, pass its filename:

```powershell
.\DevelopmentTools\updateDatabase.cmd "C:/Data/JobsDb/review.db"
```

Relative filenames resolve from `SourceCode/`, where the updater runs. Prefer absolute paths; an explicitly selected parent directory must already exist. EF command options such as `--connection` are no longer accepted. The normal build forwards the optional filename to this SQL phase, then rebuilds the solution and runs all tests, stopping at the first failure.

These commands preserve records and do not import postings or install example filters. Application startup calls library readiness validation once. It checks the SQL version manifest and mapped queries, reports missing or incompatible databases with an updater instruction, and performs no schema upgrades or database creation.

## Job Field Contract

`title`, `company`, `source`, `sourceUrl`, and `sourceJobId` must not be NULL. Descriptive fields `location`, `description`, `requirements`, `salaryCurrency`, `jobType`, `remoteType`, and `notes` may be NULL when not supplied. Numeric salary fields were already optional. No placeholder text is inserted for missing details.

The optional-details migration preserves existing values. Reverting to the preceding schema requires filling any NULL details first; rollback does not silently replace unknown values with empty strings. Search-filter, credential, and scraper-log field contracts remain separate cleanup tasks.
