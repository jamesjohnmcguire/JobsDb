# Database Schema

`schema.sql` shows the complete current SQLite schema: tables, columns, defaults, primary keys, and indexes. It also records the applied EF migration baseline so databases created from SQL can subsequently use application initialization and migrations.

From the repository root, create a new empty database with the SQLite command-line tool:

```sh
sqlite3 -bail recreated.db ".read SourceCode/Database/schema.sql"
```

Use a new database file. This bootstrap script creates the schema; it does not reset or upgrade an existing database. It contains no scraped jobs, credentials, or sample search filters. No `staticData.sql` is currently needed.

EF Core migrations remain responsible for upgrades. Update this snapshot alongside schema migrations. The database-schema tests compare the SQL-created schema and migration history with a migrated database, then verify repeated application initialization preserves existing data.

Application tables use PascalCase and columns use camelCase. The snapshot is exported from a newly migrated temporary database, so it shows final table definitions rather than historical alterations. A repeatable export command remains a follow-up task.

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
