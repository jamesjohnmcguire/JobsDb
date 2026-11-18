/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobRefreshService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Refreshes persisted job listings through the library's sources.
/// Only one refresh session may run at a time per library manager.
/// </summary>
public interface IJobRefreshService
{
	/// <summary>
	/// Runs all configured sources and waits for their results to be saved.
	/// </summary>
	/// <returns>The completed source results.</returns>
	Task<List<ScraperResult>> RefreshJobsAsync();

	/// <summary>
	/// Runs one configured source and waits for its result to be saved.
	/// </summary>
	/// <param name="source">The configured source name.</param>
	/// <returns>The completed source result.</returns>
	Task<ScraperResult> RefreshSourceAsync(string source);
}
