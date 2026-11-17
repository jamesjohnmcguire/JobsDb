# JobsDb
A Dashboard for Relevant Employment Opportunities for You

## Development Commands

Install the .NET 10 SDK and initialize dependencies with `git submodule update --init --recursive`. Restore the repository-local EF tool with `dotnet tool restore` before using the build or database-update scripts.

| Task | Windows | Bash |
| --- | --- | --- |
| Update database, rebuild, and test | `.\DevelopmentTools\build.cmd` | `bash DevelopmentTools/build.sh` |
| Rebuild a development database from SQL | `.\DevelopmentTools\rebuildDatabase.cmd review.db` | `bash DevelopmentTools/rebuildDatabase.sh review.db` |
| Apply database migrations | `.\DevelopmentTools\updateDatabase.cmd` | `bash DevelopmentTools/updateDatabase.sh` |
| Run all tests (Debug) | `.\DevelopmentTools\test.cmd` | `bash DevelopmentTools/test.sh` |
| Clean solution (Debug) | `.\DevelopmentTools\clean.cmd` | `bash DevelopmentTools/clean.sh` |

Run these from the repository root, or invoke the scripts by their full path from another directory. The build workflow updates the database, rebuilds the solution in Debug, then runs all tests. It stops at the first failure and returns that stage's exit code. The rebuild uses `--target:Rebuild` with automatic package restore and default verbosity. The updater first builds the library containing the migrations, then applies migrations with EF's `--no-build`; the full solution rebuild follows the database update. The standalone update, test, and clean scripts remain available. No platform overrides are necessary for the current project defaults.

The full solution includes WPF and Access integration tests, so use Windows with the x64 Microsoft Access Database Engine used by CI. The Bash scripts can run under Git Bash on Windows. Build warnings remain visible.

The build and database-update scripts target `%LOCALAPPDATA%\JobsDb\jobsdb.db` by default. Arguments passed to build are forwarded only to its database-update stage. For example: `.\DevelopmentTools\build.cmd --connection "Data Source=C:/Data/JobsDb/review.db"`. In Bash, use `bash DevelopmentTools/build.sh --connection "Data Source=C:/Data/JobsDb/review.db"`. The selected directory must already exist. See [database instructions](SourceCode/Database/README.md) for details. Reference-data initialization and data preparation remain follow-up changes.
