/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContextFactory.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Creates design-time contexts using the application's default database
/// configuration. EF tooling can override the connection when required.
/// </summary>
public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
	/// <summary>
	/// Creates a context for EF migration tooling.
	/// </summary>
	/// <param name="args">Arguments supplied by EF tooling.</param>
	/// <returns>A context using the default application database.</returns>
	public JobsDbContext CreateDbContext(string[] args)
	{
		return new JobsDbContext();
	}
}
