# SQL-First Mapping Experiment

## Result

The isolated experiment successfully created SQLite from `schema.sql` and generated an EF context and four storage classes. A nullable `importBatch` TEXT column added only to a disposable database appeared as `string? ImportBatch` with its column mapping on the next scaffold. SQL can drive structural generation without a duplicate handwritten property declaration.

Stock scaffolding does not preserve the application's C# types. Do not adopt its output as a replacement for the existing models or context. This completes the initial investigation; integration with the existing repositories is still pending.

## Observed Differences

| Schema detail | Generated result | Existing application contract |
| --- | --- | --- |
| Date columns declared TEXT | `string` or `string?` | `DateTime` or `DateTime?` |
| Salary columns declared TEXT | `string?` | `decimal?` |
| Status declared TEXT | `string` | `ApplicationStatus` with a string conversion |
| Boolean columns declared INTEGER | `int` | `bool` |
| Credentials table | `Credential` class | `ScraperCredential` class |
| Nullability, indexes, column names, SQL defaults | Present in generated mappings | Must continue to follow SQL |

C# maximum-length metadata in the current configuration was not reconstructed from TEXT declarations. Those lengths are not SQL CHECK constraints in the current schema. Keep application validation explicit; generation must not invent database guarantees.

The scaffold included only `Jobs`, `Credentials`, `SearchFilters`, and `ScraperLogs`. EF history and SQLite internal tables were excluded. `--no-onconfiguring` kept connection strings out of the generated context; connection ownership remains an infrastructure responsibility.

## Architecture Decision

Keep application-facing types and repository contracts stable. Generated storage representations belong inside infrastructure. Before adopting generation, prove the smallest maintainable conversion approach for dates, decimal values, booleans, and enums. Compare isolated persistence adapters with supported generator customization; do not introduce a custom schema parser or overwrite domain classes to bypass these differences.

Once those conversions are proven, add the lean schema-preparation scripts. Do not wire stock scaffolding directly into the normal build yet. Upgrade migrations remain separate, reviewed changes derived from the SQL-defined target.

## Repeat the Experiment

Run from the repository root in PowerShell, with SQLite and the pinned EF tool available:

```powershell
$reviewPath = Join-Path $env:TEMP ('JobsDb-schema-review-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $reviewPath
sqlite3 -bail "$reviewPath/schema.db" ".read SourceCode/Database/schema.sql"
dotnet ef dbcontext scaffold "Data Source=$reviewPath/schema.db" Microsoft.EntityFrameworkCore.Sqlite --project SourceCode/DigitalZenWorks.JobsDb.Library --configuration Debug --context SchemaContext --context-dir "$reviewPath/Generated" --output-dir "$reviewPath/Generated" --namespace JobsDb.SchemaExperiment --context-namespace JobsDb.SchemaExperiment --no-onconfiguring --table Jobs --table Credentials --table SearchFilters --table ScraperLogs
```

Inspect `Generated/SchemaContext.cs` and the four classes. The experiment used .NET SDK 11.0.100 with EF Core 10.0.11 and the existing net10.0 project. No generated files, temporary databases, or experimental context were added to the solution.

## Regression Coverage

`DatabaseSchemaTests.Schema_JobRepositoryPreservesTypedValues` loads the committed SQL directly into an isolated database and exercises the existing repository without migrations or `EnsureCreated`. Two cases cover archived and active jobs with different application statuses. They verify generated identifiers, decimal precision, timestamps, optional NULL fields, enum values, notes, and subsequent updates after clearing tracked entities.

These tests protect current typed behavior; they do not claim the scaffolded context can replace the existing one. Runtime code, the master SQL, existing migrations, the default application database, and scraper behavior remain unchanged.
