namespace JobsDbLibrary.Scrapers;

using JobsDb.Core.Data;
using JobsDb.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			.ToListAsync();
	}

	public async Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50)
	{
		return await _context.ScraperLogs
			.Where(l => l.Source == source)
			.OrderByDescending(l => l.Timestamp)
			.Take(count)
			.ToListAsync();
	}

	public async Task<ScraperLog> AddAsync(ScraperLog log)
	{
		_context.ScraperLogs.Add(log);
		await _context.SaveChangesAsync();
		return log;
	}
}
