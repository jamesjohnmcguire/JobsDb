/////////////////////////////////////////////////////////////////////////////
// <copyright file="IScraperLogRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Defines access to stored scraper run logs.
/// </summary>
public interface IScraperLogRepository
{
	/// <summary>
	/// Reads the newest acquisition logs across all sources.
	/// </summary>
	/// <param name="count">The maximum number of entries to return.</param>
	/// <returns>The requested logs in descending timestamp order.</returns>
	Task<List<ScraperLog>> GetRecentLogsAsync(int count = 50);

	/// <summary>
	/// Reads the newest acquisition logs for one source.
	/// </summary>
	/// <param name="source">The source identifier.</param>
	/// <param name="count">The maximum number of entries to return.</param>
	/// <returns>Matching logs in descending timestamp order.</returns>
	Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50);

	/// <summary>
	/// Inserts a scraper run log.
	/// </summary>
	/// <param name="log">The log to record.</param>
	/// <returns>The saved log with its database identifier.</returns>
	Task<ScraperLog> AddAsync(ScraperLog log);
}
