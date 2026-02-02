using JobsDbLibrary.Scrapers;

namespace JobsDb.Core.Repositories
{
	using JobsDbLibrary.Scrapers;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public interface IScraperLogRepository
	{
		Task<List<ScraperLog>> GetRecentLogsAsync(int count = 50);
		Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50);
		Task<ScraperLog> AddAsync(ScraperLog log);
	}
}

