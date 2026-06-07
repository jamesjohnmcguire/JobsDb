/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobScraperBase.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Scrapers;

using System;
using System.Threading.Tasks;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDbLibrary.Scrapers;

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
	// public abstract Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null);

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
			await credentialRepository.GetBySourceAsync(sourceName).ConfigureAwait(false);

		return credential;
	}

	/// <summary>
	/// Add a new job or update an existing one based on SourceJobId
	/// </summary>
	protected async Task<Job> AddOrUpdateJobAsync(Job job)
	{
		var existing = await jobRepository.GetBySourceIdAsync(job.Source, job.SourceJobId).ConfigureAwait(false);

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

			return await jobRepository.UpdateAsync(existing).ConfigureAwait(false);
		}
		else
		{
			job.Source = sourceName;
			job.DateScraped = DateTime.UtcNow;
			job.Status = ApplicationStatus.NotApplied;
			return await jobRepository.AddAsync(job).ConfigureAwait(false);
		}
	}

	public virtual async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
	{
		var result = new ScraperResult();

		return result;
	}
}
