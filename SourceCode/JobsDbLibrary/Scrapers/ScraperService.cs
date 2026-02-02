using JobsDb.Core.Data;
using JobsDb.Core.Scrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsDbLibrary.Scrapers
{
	/// <summary>
	/// Service to manage and run multiple scrapers
	/// </summary>
	public class ScraperService
	{
		private readonly Dictionary<string, JobScraperBase> _scrapers;
		private readonly JobsDbContext _context;

		public ScraperService(JobsDbContext context)
		{
			_context = context;
			_scrapers = new Dictionary<string, JobScraperBase>();
		}

		/// <summary>
		/// Register a scraper for a specific source
		/// </summary>
		public void RegisterScraper(string source, JobScraperBase scraper)
		{
			_scrapers[source] = scraper;
		}

		/// <summary>
		/// Run a specific scraper by source name
		/// </summary>
		public async Task<ScraperResult> RunScraperAsync(string source, SearchFilter filter = null)
		{
			if (!_scrapers.ContainsKey(source))
			{
				return new ScraperResult
				{
					Success = false,
					ErrorMessage = $"No scraper registered for source: {source}"
				};
			}

			var startTime = DateTime.UtcNow;
			var scraper = _scrapers[source];
			var result = await scraper.ScrapeJobsAsync(filter);

			// Log the scraping activity
			var log = new ScraperLog
			{
				Source = source,
				Timestamp = startTime,
				JobsFound = result.JobsFound,
				JobsAdded = result.JobsAdded,
				JobsUpdated = result.JobsUpdated,
				Success = result.Success,
				ErrorMessage = result.ErrorMessage,
				DurationMs = (int)result.Duration.TotalMilliseconds
			};

			_context.ScraperLogs.Add(log);
			await _context.SaveChangesAsync();

			return result;
		}

		/// <summary>
		/// Run all registered scrapers
		/// </summary>
		public async Task<List<ScraperResult>> RunAllScrapersAsync()
		{
			var results = new List<ScraperResult>();

			foreach (var source in _scrapers.Keys)
			{
				var result = await RunScraperAsync(source);
				results.Add(result);
			}

			return results;
		}

		/// <summary>
		/// Get list of registered scraper sources
		/// </summary>
		public List<string> GetRegisteredSources()
		{
			return _scrapers.Keys.ToList();
		}
	}
}
