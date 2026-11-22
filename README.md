# JobsDb
A Dashboard for Relevant Employment Opportunities for You

## Development Commands

Install the .NET 10 SDK and initialize dependencies with `git submodule update --init --recursive`. Install `sqlite3` on PATH for the database scripts.

| Task | Windows | Bash |
| --- | --- | --- |
| Update database, rebuild, and test | `.\DevelopmentTools\build.cmd` | `bash DevelopmentTools/build.sh` |
| Rebuild a development database from SQL | `.\DevelopmentTools\rebuildDatabase.cmd review.db` | `bash DevelopmentTools/rebuildDatabase.sh review.db` |
| Apply SQL upgrades | `.\DevelopmentTools\updateDatabase.cmd` | `bash DevelopmentTools/updateDatabase.sh` |
| Run all tests (Debug) | `.\DevelopmentTools\test.cmd` | `bash DevelopmentTools/test.sh` |
| Clean solution (Debug) | `.\DevelopmentTools\clean.cmd` | `bash DevelopmentTools/clean.sh` |

Run these from the repository root, or invoke the scripts by their full path from another directory. The build workflow updates the database, rebuilds the solution in Debug, then runs all tests. It stops at the first failure and returns that stage's exit code. The rebuild uses `--target:Rebuild` with automatic package restore and default verbosity. The database stage runs `sqlite3` directly, without compiling or loading application code. The standalone update, test, and clean scripts remain available. No platform overrides are necessary for the current project defaults.

The full solution includes WPF and Access integration tests, so use Windows with the x64 Microsoft Access Database Engine used by CI. The Bash scripts can run under Git Bash on Windows. Build warnings remain visible.

The build and database-update scripts target `%LOCALAPPDATA%\JobsDb\jobsdb.db` by default. An optional database filename passed to build is forwarded only to its database-update stage. For example: `.\DevelopmentTools\build.cmd "C:/Data/JobsDb/review.db"`. In Bash, use `bash DevelopmentTools/build.sh "C:/Data/JobsDb/review.db"`. The selected directory must already exist. See [database instructions](SourceCode/Database/README.md) for details. Required `staticData.sql` is loaded when present; data imports remain separate.

## Run the Applications

Prepare the application database with `DevelopmentTools/updateDatabase.cmd` before the first import. From the repository root:

```text
dotnet run --project SourceCode/JobsDb.CommandLine -- help
dotnet run --project SourceCode/JobsDb.CommandLine -- import "Emails"
dotnet run --project SourceCode/JobsDbApplication
```

The console executable is `jobsdb`; the desktop executable is `JobsDbApplication`. Import currently supports LinkedIn HTML digests. It leaves export files unchanged, reports ignored/failed/review-needed items, and deduplicates stored jobs across repeated runs. Email-body storage and full-posting enrichment remain later steps.
