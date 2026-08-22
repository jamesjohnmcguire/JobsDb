using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// Tells the EF Core CLI tools how to construct a JobsDbContext at design time
/// (i.e. when you run `dotnet ef migrations add ...` or `dotnet ef database update`).
///
/// This is only used by the tooling — never referenced by your running application.
/// It's required because JobsDbContext doesn't have a plain parameterless constructor
/// that resolves a connection string on its own (it builds the path in its constructor).
/// </summary>
public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
	public JobsDbContext CreateDbContext(string[] args)
	{
		// Use a fixed, predictable path for design-time operations so that
		// migrations are generated against a consistent schema baseline.
		// This does NOT have to be the same file your app uses at runtime -
		// it's just used to scaffold/apply migrations.
		var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var appFolder = Path.Combine(folder, "JobsDb");
		Directory.CreateDirectory(appFolder);
		var dbPath = Path.Combine(appFolder, "jobsdb.db");

		var optionsBuilder = new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite($"Data Source={dbPath}");

		return new JobsDbContext(optionsBuilder.Options);
	}
}
