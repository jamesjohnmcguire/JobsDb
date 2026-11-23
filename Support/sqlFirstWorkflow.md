# SQL-First Database Workflow

`SourceCode/Database/schema.sql` is the authoritative schema. Edit it directly and add ordered SQL upgrades for existing databases. Keep application table names in PascalCase and columns in camelCase. Use `"id" INTEGER PRIMARY KEY AUTOINCREMENT` as the target declaration. Add `staticData.sql` only for required reference data and make it safe to reapply.

Prepare the database before building or testing. The native updater loads the master schema for a new database or applies pending upgrades to an existing one. Each upgrade records its version with its changes. Apply required reference data, then check SQLite integrity and foreign keys. Keep destructive rebuilds separate and explicitly targeted. Library startup validates readiness.

EF Core is the current ORM. SQL upgrades run independently of EF and application compilation. Retain the existing EF migration IDs for compatibility with installed databases. Review rename intent, backfills, and data transformations in upgrade SQL.

SQLite scaffolding can recover structural mappings but infers strings for TEXT dates and salaries and integers for INTEGER booleans. It also loses enum conversions and application-only validation metadata. Keep handwritten typed contracts and conversions stable. Any generated storage types belong inside data access, and generation remains optional.

See [open tasks](Tasks.md) for schema and data-access work and the [database README](../SourceCode/Database/README.md) for commands.
