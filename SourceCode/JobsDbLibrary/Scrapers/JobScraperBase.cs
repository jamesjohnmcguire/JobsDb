namespace JobsDb.Core.Scrapers
//namespace JobsDb.Core.Services
{
	using HtmlAgilityPack;
	using JobsDb.Core.Models;
	using JobsDb.Core.Repositories;
	using JobsDb.Core.Scrapers;
	using JobsDbLibrary.Scrapers;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Base class for all job scrapers providing common functionality
	/// </summary>
	public abstract class JobScraperBase
	{
		protected readonly ICredentialRepository credentialRepository;
		protected readonly IJobRepository jobRepository;
		protected readonly string sourceName;

		protected JobScraperBase(
			IJobRepository jobRepository,
			ICredentialRepository credentialRepository,
			string sourceName)
		{
			this.jobRepository = jobRepository;
			this.credentialRepository = credentialRepository;
			this.sourceName = sourceName;
		}

		/// <summary>
		/// Login to the source if required. Must be implemented by derived classes.
		/// </summary>
		protected abstract Task<bool> LoginAsync(ScraperCredential credential);

		/// <summary>
		/// Scrape jobs from the source. Must be implemented by derived classes.
		/// </summary>
//		public abstract Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null);

		public ICredentialRepository CredentialRepository
		{
			get { return credentialRepository; }
		}

		public IJobRepository JobRepository
		{
			get { return jobRepository; }
		}

		public string SourceName
		{
			get { return sourceName; }
		}

		/// <summary>
		/// Get credentials for this scraper's source from the database
		/// </summary>
		protected async Task<ScraperCredential> GetCredentialsAsync()
		{
			ScraperCredential credential =
				await credentialRepository.GetBySourceAsync(sourceName);

			return credential;
		}

		/// <summary>
		/// Add a new job or update an existing one based on SourceJobId
		/// </summary>
		protected async Task<Job> AddOrUpdateJobAsync(Job job)
		{
			var existing = await jobRepository.GetBySourceIdAsync(job.Source, job.SourceJobId);

			if (existing != null)
			{
				// Update only if certain fields changed
				existing.Title = job.Title;
				existing.Company = job.Company;
				existing.Location = job.Location;
				existing.Description = job.Description;
				existing.Requirements = job.Requirements;
				existing.SalaryMin = job.SalaryMin;
				existing.SalaryMax = job.SalaryMax;
				existing.SalaryCurrency = job.SalaryCurrency;
				existing.JobType = job.JobType;
				existing.RemoteType = job.RemoteType;
				existing.DateScraped = DateTime.UtcNow;

				return await jobRepository.UpdateAsync(existing);
			}
			else
			{
				job.Source = sourceName;
				job.DateScraped = DateTime.UtcNow;
				job.Status = ApplicationStatus.NotApplied;
				return await jobRepository.AddAsync(job);
			}
		}

		public virtual async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
		{
			var result = new ScraperResult();

			return result;
		}
	}
}
