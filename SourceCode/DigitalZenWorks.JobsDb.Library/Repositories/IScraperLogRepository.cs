/////////////////////////////////////////////////////////////////////////////
// <copyright file="IScraperLogRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

public interface IScraperLogRepository
{
	Task<List<ScraperLog>> GetRecentLogsAsync(int count = 50);

	Task<List<ScraperLog>> GetLogsBySourceAsync(string source, int count = 50);

	Task<ScraperLog> AddAsync(ScraperLog log);
}
