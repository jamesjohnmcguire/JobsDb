/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperLogRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Stores acquisition run logs in the JobsDb database.
/// </summary>
public class ScraperLogRepository : IScraperLogRepository
{
	private readonly JobsDbContext context;

	/// <summary>
	/// Initializes a new instance of the <see cref="ScraperLogRepository"/>
	/// class.
	/// </summary>
	/// <param name="context">
	/// The database context supplied by the caller.
	/// </param>
	public ScraperLogRepository(JobsDbContext context)
	{
		this.context = context;
	}

	/// <inheritdoc/>
	public async Task<List<ScraperLog>> GetRecentLogsAsync(int count = 50)
	{
		return await context.ScraperLogs
			.OrderByDescending(l => l.Timestamp)
			.Take(count)
			.ToListAsync().ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50)
	{
		return await context.ScraperLogs
			.Where(l => l.Source == source)
			.OrderByDescending(l => l.Timestamp)
			.Take(count)
			.ToListAsync().ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<ScraperLog> AddAsync(ScraperLog log)
	{
		context.ScraperLogs.Add(log);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return log;
	}
}
