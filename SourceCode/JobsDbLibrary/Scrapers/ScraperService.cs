/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbLibrary.Scrapers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Scrapers;

/// <summary>
/// Service to manage and run multiple scrapers.
/// </summary>
public class ScraperService
{
	private readonly Dictionary<string, JobScraperBase> scrapers;
	private readonly JobsDbContext context;

	public ScraperService(JobsDbContext context)
	{
		this.context = context;
		scrapers = new Dictionary<string, JobScraperBase>();
	}

	/// <summary>
	/// Register a scraper for a specific source.
	/// </summary>
	public void RegisterScraper(string source, JobScraperBase scraper)
	{
		scrapers[source] = scraper;
	}

	/// <summary>
	/// Run a specific scraper by source name.
	/// </summary>
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
	public List<string> GetRegisteredSources()
	{
		return scrapers.Keys.ToList();
	}
}
