# JobsDb
A Dashboard for Relevant Employment Opportunities for You

## Development Commands

Install the .NET 10 SDK and initialize dependencies with `git submodule update --init --recursive`.

| Task | Windows | Bash |
| --- | --- | --- |
| Rebuild solution (Debug) | `.\DevelopmentTools\build.cmd` | `bash DevelopmentTools/build.sh` |
| Run all tests (Debug) | `.\DevelopmentTools\test.cmd` | `bash DevelopmentTools/test.sh` |
| Clean solution (Debug) | `.\DevelopmentTools\clean.cmd` | `bash DevelopmentTools/clean.sh` |

Run these from the repository root, or invoke the scripts by their full path from another directory. Each script runs one .NET command at default verbosity and returns its exit code. Build uses `--target:Rebuild` and restores packages automatically; test restores and builds as needed. No platform overrides are necessary for the current project defaults.

The full solution includes WPF and Access integration tests, so use Windows with the x64 Microsoft Access Database Engine used by CI. The Bash scripts can run under Git Bash on Windows. Build warnings remain visible.
