/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperLogRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbLibrary.Scrapers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Repositories;
using Microsoft.EntityFrameworkCore;

public class ScraperLogRepository : IScraperLogRepository
{
	private readonly JobsDbContext _context;

	public ScraperLogRepository(JobsDbContext context)
	{
		_context = context;
	}

	public async Task<List<ScraperLog>> GetRecentLogsAsync(int count = 50)
	{
		return await _context.ScraperLogs
			.OrderByDescending(l => l.Timestamp)
			.Take(count)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50)
	{
		return await _context.ScraperLogs
			.Where(l => l.Source == source)
			.OrderByDescending(l => l.Timestamp)
			.Take(count)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<ScraperLog> AddAsync(ScraperLog log)
	{
		_context.ScraperLogs.Add(log);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return log;
	}
}
