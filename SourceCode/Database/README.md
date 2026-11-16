# Database Schema

`schema.sql` shows the complete current SQLite schema: tables, columns, defaults, primary keys, and indexes. It also records the applied EF migration baseline so databases created from SQL can subsequently use application initialization and migrations.

From the repository root, create a new empty database with the SQLite command-line tool:

```sh
sqlite3 -bail recreated.db ".read SourceCode/Database/schema.sql"
```

Use a new database file. This bootstrap script creates the schema; it does not reset or upgrade an existing database. It contains no scraped jobs, credentials, or sample search filters. No `staticData.sql` is currently needed.

EF Core migrations remain responsible for upgrades. Update this snapshot alongside schema migrations. The database-schema tests compare the SQL-created schema and migration history with a migrated database, then verify repeated application initialization preserves existing data.

The snapshot currently comes from the initial migration. A repeatable export command is the next iteration; once there are later structural migrations, the snapshot must continue to show final table definitions rather than accumulate historical alterations.
