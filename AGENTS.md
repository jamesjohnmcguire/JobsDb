# Repository Guidelines

## Project Structure & Module Organization

`SourceCode/JobsDb.slnx` contains the solution. Core projects are:

- `DigitalZenWorks.JobsDb.Library/`: models, repositories, scrapers, configuration, EF Core SQLite data access, and migrations.
- `DigitalZenWorks.JobsDb.Application/`: WPF desktop UI; XAML files define the interface.
- `JobsDbConsole/`: console entry point for scraper diagnostics.
- `JobsDbTests/`: application and library tests.
- `Database/Schema.sql`: database schema reference.

`SourceCode/Database.ToolKit`, `HttpTool`, and `UtilitiesNet` are Git submodules. Keep dependency changes scoped and review submodule pointer updates explicitly. `Support/` contains development notes.

## Build, Test, and Development Commands

Use Windows with the .NET 10 SDK. Run commands from the repository root:

- `git submodule update --init --recursive`: initialize dependencies.
- `dotnet restore SourceCode/JobsDb.slnx`: restore NuGet packages.
- `dotnet build SourceCode/JobsDb.slnx --no-restore`: build all projects and run analyzers.
- `dotnet test SourceCode/JobsDb.slnx --no-build --verbosity normal`: run the complete suite after building.
- `dotnet run --project SourceCode/DigitalZenWorks.JobsDb.Application`: launch the desktop app.
- `dotnet run --project SourceCode/JobsDbConsole`: run scraper diagnostics against TokyoDev.

CI runs on Windows and installs the x64 Microsoft Access Database Engine for dependency tests; full local suite runs may require it too.

## Coding Style & Naming Conventions

Follow `.editorconfig`: tabs with size four, an 80-character line limit, UTF-8, final newlines, and no trailing whitespace. C# and project files use LF; batch scripts use CRLF. Prefer explicit types, braces, explicit accessibility, and file-scoped namespaces. Match class and file names; use PascalCase for types and members, and camelCase for locals and instance fields. Preserve copyright headers. Resolve applicable .NET, StyleCop, and Roslynator analyzer diagnostics during builds.

## Testing Guidelines

Use NUnit, Moq, and `Assert.That` assertions. Name fixtures `*Tests.cs` and methods descriptively, such as `SearchFilter_NewInstance_HasDefaultValues`. Cover changed behavior and regressions; isolate database tests with temporary files and cleanup. Run focused tests with `dotnet test SourceCode/JobsDbTests`. Coverlet is available; no numeric coverage threshold is configured.

## Commit & Pull Request Guidelines

Recent commits use short, imperative, title-style subjects such as `Clean Up Scraper Code`. Keep commits focused. Pull requests should describe the behavior changed, link relevant issues, and report validation. Include screenshots for WPF changes and explain database migrations or submodule updates.
