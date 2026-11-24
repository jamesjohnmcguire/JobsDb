/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Services;

/// <summary>
/// Service to manage and run multiple scrapers.
/// </summary>
internal sealed class ScraperService : IJobRefreshService
{
	private readonly Dictionary<string, JobScraperBase> scrapers;
	private readonly JobsDbContext context;

	/// <summary>
	/// Initializes a new instance of the <see cref="ScraperService"/> class.
	/// </summary>
	/// <param name="context">
	/// The database context for this refresh session.
	/// </param>
	public ScraperService(JobsDbContext context)
	{
		this.context = context;
		scrapers = new Dictionary<string, JobScraperBase>();
	}

	/// <inheritdoc/>
	public Task<List<ScraperResult>> RefreshJobsAsync()
	{
		return RunAllScrapersAsync();
	}

	/// <inheritdoc/>
	public Task<ScraperResult> RefreshSourceAsync(string source)
	{
		return RunScraperAsync(source);
	}

	/// <summary>
	/// Register a scraper for a specific source.
	/// </summary>
	/// <param name="source">The name used to select this scraper.</param>
	/// <param name="scraper">
	/// The acquisition implementation for the source.
	/// </param>
	public void RegisterScraper(string source, JobScraperBase scraper)
	{
		scrapers[source] = scraper;
	}

	/// <summary>
	/// Run a specific scraper by source name.
	/// </summary>
	/// <param name="source">The name of a registered source.</param>
	/// <param name="filter">Optional criteria to pass to the scraper.</param>
	/// <returns>
	/// The run result, including a failure for an unregistered source.
	/// </returns>
	public async Task<ScraperResult> RunScraperAsync(string source, SearchFilter filter = null)
	{
		if (!scrapers.ContainsKey(source))
		{
			return new ScraperResult
			{
				Success = false,
				ErrorMessage = $"No scraper registered for source: {source}"
			};
		}

		var startTime = DateTime.UtcNow;
		var scraper = scrapers[source];
		var result = await scraper.ScrapeJobsAsync(filter).ConfigureAwait(false);

		// Log the scraping activity
		ScraperLog log = new ScraperLog
		{
			Source = source,
			Timestamp = startTime,
			JobsFound = result.JobsFound,
			JobsAdded = result.JobsAdded,
			JobsUpdated = result.JobsUpdated,
			Success = result.Success,
			ErrorMessage = result.ErrorMessage ?? string.Empty,
			DurationMs = (int)result.Duration.TotalMilliseconds
		};

		context.ScraperLogs.Add(log);
		await context.SaveChangesAsync().ConfigureAwait(false);

		return result;
	}

	/// <summary>
	/// Run all registered scrapers.
	/// </summary>
	/// <returns>One run result for each registered source.</returns>
	public async Task<List<ScraperResult>> RunAllScrapersAsync()
	{
		List<ScraperResult> results = new List<ScraperResult>();

		foreach (var source in scrapers.Keys)
		{
			var result = await RunScraperAsync(source).ConfigureAwait(false);
			results.Add(result);
		}

		return results;
	}

	/// <summary>
	/// Get list of registered scraper sources.
	/// </summary>
	/// <returns>The names of registered sources.</returns>
	public List<string> GetRegisteredSources()
	{
		return scrapers.Keys.ToList();
	}
}
