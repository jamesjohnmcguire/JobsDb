/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobScraperBase.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;

/// <summary>
/// Base class for all job scrapers providing common functionality.
/// </summary>
public abstract class JobScraperBase
{
	/// <summary>
	/// The repository used to look up source credentials.
	/// </summary>
	protected readonly ICredentialRepository credentialRepository;
	/// <summary>
	/// The repository used to read and save listings.
	/// </summary>
	protected readonly IJobRepository jobRepository;
	/// <summary>
	/// The identifier associated with this source.
	/// </summary>
	protected readonly string sourceName;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobScraperBase"/> class.
	/// </summary>
	/// <param name="jobRepository">The repository for stored listings.</param>
	/// <param name="credentialRepository">
	/// The repository for stored source credentials.
	/// </param>
	/// <param name="sourceName">
	/// The identifier for the scraper's source.
	/// </param>
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
	/// <param name="credential">The account information for the source.</param>
	/// <returns>
	/// True when sign-in succeeds or is unnecessary, otherwise false.
	/// </returns>
	protected abstract Task<bool> LoginAsync(ScraperCredential credential);

	/// <summary>
	/// Gets the repository used to look up source credentials.
	/// </summary>
	public ICredentialRepository CredentialRepository
	{
		get { return credentialRepository; }
	}

	/// <summary>
	/// Gets the repository used to read and save listings.
	/// </summary>
	public IJobRepository JobRepository
	{
		get { return jobRepository; }
	}

	/// <summary>
	/// Gets the identifier for the scraper's source.
	/// </summary>
	public string SourceName
	{
		get { return sourceName; }
	}

	/// <summary>
	/// Get credentials for this scraper's source from the database.
	/// </summary>
	/// <returns>
	/// The stored credential, or null when no credential exists.
	/// </returns>
	protected async Task<ScraperCredential> GetCredentialsAsync()
	{
		ScraperCredential credential =
			await credentialRepository.GetBySourceAsync(sourceName).ConfigureAwait(false);

		return credential;
	}

	/// <summary>
	/// Add a new job or update an existing one based on SourceJobId.
	/// </summary>
	/// <param name="job">The source listing with the values to save.</param>
	/// <returns>The listing after the insert or update.</returns>
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

	/// <summary>
	/// Retrieves job listings for this source. The base implementation returns
	/// an empty result.
	/// </summary>
	/// <param name="filter">Optional source-specific search criteria.</param>
	/// <returns>The acquisition outcome reported by the scraper.</returns>
	public virtual async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
	{
		ScraperResult result = new ScraperResult();

		return result;
	}
}
