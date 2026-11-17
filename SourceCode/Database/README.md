# Database Schema

`schema.sql` is the authoritative current SQLite schema: tables, columns, defaults, primary keys, and indexes. Edit this file directly; do not regenerate it from EF models. It also records the applied EF migration baseline so databases created from SQL can subsequently use application initialization and migrations.

With `sqlite3` on PATH, rebuild a development database from the repository root:

```powershell
.\DevelopmentTools\rebuildDatabase.cmd review.db
```

For Bash, use `bash DevelopmentTools/rebuildDatabase.sh review.db`. The explicit destination is replaced, discarding its existing data; `jobsdb.db` is reserved for the application and refused in any directory. Close database connections first. Relative destinations resolve from your terminal's working directory, and the parent directory must exist. A temporary database is loaded before replacement, so SQL-loading failures leave the existing database intact. The normal build never calls rebuild.

Rebuild loads `schema.sql` and then `staticData.sql` if present. No required static-data file is currently needed. Scraped jobs, credentials, and optional sample filters are not installed. No EF generation or compilation is needed.

EF Core migrations currently remain responsible for upgrades. Rebuild loads SQL edits into the selected development database; the updater does not yet derive upgrades or C# mappings from those edits. The [SQL-first transition plan](../../Support/sqlFirstWorkflow.md) defines how mappings and reviewed upgrade migrations will follow the SQL. The database-schema tests compare the SQL-created schema and migration history with a migrated database, then verify repeated application initialization preserves existing data.

Application tables use PascalCase and columns use camelCase. Maintain final table and index definitions here; keep historical upgrade steps in migrations.

Runtime and EF tooling share the default database path, `%LOCALAPPDATA%\JobsDb\jobsdb.db`. A path-based context resolves relative filenames against the current working directory and quotes connection-string characters safely. Supplied EF options and later connection overrides determine the reported data source; in-memory contexts report their configured memory source.

## SQL Upgrade History

The reviewed SQL files in `Upgrades/` follow `upgradeOrder.txt`, which lists the existing version ID and SQL filename on each line. A runner must skip recorded versions; each file writes its history entry in the same transaction as its schema changes. These files establish the SQL upgrade contract; the current updater still executes EF migrations until the next transition step.

## Updating a Database

Restore the pinned EF tool once from the repository root:

```sh
dotnet tool restore
```

Run `DevelopmentTools/updateDatabase.cmd` on Windows or `bash DevelopmentTools/updateDatabase.sh` in Bash. The scripts first build the library containing the migrations in Debug, then apply pending migrations with EF's `--no-build` to the default `%LOCALAPPDATA%\JobsDb\jobsdb.db` database, preserving existing records. Repeating the command applies no migrations when the database is already current.

To select a different database, pass EF's native connection option. For example, from PowerShell:

```powershell
.\DevelopmentTools\updateDatabase.cmd --connection "Data Source=C:/Data/JobsDb/review.db"
```

The destination directory must already exist when overriding the connection. Relative connection paths resolve from `SourceCode/`, where the scripts run. Prefer an absolute path when selecting another database. The scripts can be invoked by full path from any working directory and return the failing step's exit code. The build workflow calls the updater first, then rebuilds the full solution and runs all tests. It forwards database arguments such as `--connection` to the updater; those arguments are passed only to EF's update command.

The update scripts preserve records; use the separate rebuild command only for disposable development data. Neither command imports postings. Required reference-data initialization will be added separately; the existing example-filter seeder is not connected to this workflow.

## Job Field Contract

`title`, `company`, `source`, `sourceUrl`, and `sourceJobId` must not be NULL. Descriptive fields `location`, `description`, `requirements`, `salaryCurrency`, `jobType`, `remoteType`, and `notes` may be NULL when not supplied. Numeric salary fields were already optional. No placeholder text is inserted for missing details.

The optional-details migration preserves existing values. Reverting to the preceding schema requires filling any NULL details first; rollback does not silently replace unknown values with empty strings. Search-filter, credential, and scraper-log field contracts remain separate cleanup tasks.
