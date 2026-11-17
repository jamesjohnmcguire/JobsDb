# Database Schema

`schema.sql` is the authoritative current SQLite schema: tables, columns, defaults, primary keys, and indexes. Edit this file directly; do not regenerate it from EF models. It also records the applied EF migration baseline so databases created from SQL can subsequently use application initialization and migrations.

With `sqlite3` on PATH, create a new database from the repository root:

```powershell
.\DevelopmentTools\createDatabase.cmd review.db
```

For Bash, use `bash DevelopmentTools/createDatabase.sh review.db`. The scripts require a new destination, refuse existing files, and read `schema.sql` directly. Relative destination paths resolve from your terminal's working directory; the parent directory must already exist. They can also be invoked by full path from another directory. No EF generation or compilation is needed.

Use a new database file. This bootstrap script creates the schema; it does not reset or upgrade an existing database. It contains no scraped jobs, credentials, or sample search filters. No `staticData.sql` is currently needed.

EF Core migrations currently remain responsible for upgrades. The creation scripts load SQL edits into new databases; the updater does not yet derive upgrades or C# mappings from those edits. The [SQL-first transition plan](../../Support/sqlFirstWorkflow.md) defines how mappings and reviewed upgrade migrations will follow the SQL. The database-schema tests compare the SQL-created schema and migration history with a migrated database, then verify repeated application initialization preserves existing data.

Application tables use PascalCase and columns use camelCase. Maintain final table and index definitions here; keep historical upgrade steps in migrations.

Runtime and EF tooling share the default database path, `%LOCALAPPDATA%\JobsDb\jobsdb.db`. A path-based context resolves relative filenames against the current working directory and quotes connection-string characters safely. Supplied EF options and later connection overrides determine the reported data source; in-memory contexts report their configured memory source.

## Updating a Database

Restore the pinned EF tool once from the repository root:

```sh
dotnet tool restore
```

Run `DevelopmentTools/updateDatabase.cmd` on Windows or `bash DevelopmentTools/updateDatabase.sh` in Bash. The scripts apply pending migrations to the default `%LOCALAPPDATA%\JobsDb\jobsdb.db` database, preserving existing records. Repeating the command applies no migrations when the database is already current.

To select a different database, pass EF's native connection option. For example, from PowerShell:

```powershell
.\DevelopmentTools\updateDatabase.cmd --connection "Data Source=C:/Data/JobsDb/review.db"
```

The destination directory must already exist when overriding the connection. Relative connection paths resolve from `SourceCode/`, where the scripts run. Prefer an absolute path when selecting another database. The scripts can be invoked by full path from any working directory and return EF's exit code. The build workflow calls the same updater after rebuilding and before running all tests. It forwards database arguments such as `--connection` and supplies `--no-build` to reuse the rebuilt assemblies.

These scripts do not rebuild an existing database or import postings. Required reference-data initialization will be added separately; the existing example-filter seeder is not connected to this workflow.

## Job Field Contract

`title`, `company`, `source`, `sourceUrl`, and `sourceJobId` must not be NULL. Descriptive fields `location`, `description`, `requirements`, `salaryCurrency`, `jobType`, `remoteType`, and `notes` may be NULL when not supplied. Numeric salary fields were already optional. No placeholder text is inserted for missing details.

The optional-details migration preserves existing values. Reverting to the preceding schema requires filling any NULL details first; rollback does not silently replace unknown values with empty strings. Search-filter, credential, and scraper-log field contracts remain separate cleanup tasks.
