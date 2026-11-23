# JobsDb

JobsDb is a Windows desktop dashboard for reviewing job listings.

## Setup and usage

To run from source, install the .NET 10 SDK and put `sqlite3` on your PATH. Run these commands from the repository root:

```powershell
git submodule update --init --recursive
.\DevelopmentTools\updateDatabase.cmd
dotnet run --project SourceCode/JobsDbApplication
```

To add listings, import a folder of LinkedIn job digest exports in HTML format:

```powershell
dotnet run --project SourceCode/JobsDb.CommandLine -- import "Emails"
```

Click **Reload Jobs** in the dashboard after importing. Select a listing and click **Open Posting** to view it in your browser.

## Contributing

Run `.\DevelopmentTools\build.cmd` to prepare the database, build, and run tests. Run `.\DevelopmentTools\test.cmd` to run tests on their own. Bash equivalents are in `DevelopmentTools/`. The full suite requires the x64 Microsoft Access Database Engine.
