/////////////////////////////////////////////////////////////////////////////
// <copyright file="LibraryManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;

/// <summary>
/// Initializes library services and owns their database lifetime.
/// </summary>
public sealed class LibraryManager : IDisposable
{
	private readonly JobsDbContext context;
	private readonly string databasePath;
	private readonly IJobRepository jobRepository;
	private readonly IJobQueryService jobQueries;
	private readonly ICredentialRepository credentialRepository;
	private readonly Lazy<IJobRefreshService> refreshService;
	private bool disposed;

	private LibraryManager(JobsDbContext context)
	{
		this.context = context;
		databasePath = Path.GetFullPath(context.GetDatabasePath());
		jobRepository = new JobRepository(context);
		DatabaseJobReader jobReader = new DatabaseJobReader(CreateReadContext);
		jobQueries = new JobQueryService(jobReader);
		credentialRepository = new CredentialRepository(context);
		refreshService = new Lazy<IJobRefreshService>(CreateRefreshService);
	}

	/// <summary>
	/// Gets the service supplying job collections.
	/// </summary>
	public IJobQueryService JobQueries
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return jobQueries;
		}
	}

	/// <summary>
	/// Gets the job repository.
	/// </summary>
	public IJobRepository Jobs
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return jobRepository;
		}
	}

	/// <summary>
	/// Gets the credential repository.
	/// </summary>
	public ICredentialRepository Credentials
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return credentialRepository;
		}
	}

	/// <summary>
	/// Gets the refresh service, preparing acquisition only when requested.
	/// </summary>
	public IJobRefreshService Refresh
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return refreshService.Value;
		}
	}

	/// <summary>
	/// Validates an existing database and prepares library services.
	/// </summary>
	/// <param name="databasePath">
	/// Database filename, or null to use the application database.
	/// </param>
	/// <returns>The initialized library manager.</returns>
	public static LibraryManager Initialize(string? databasePath = null)
	{
		JobsDbContext context;
		if (databasePath == null)
		{
			context = new JobsDbContext();
		}
		else
		{
			context = new JobsDbContext(databasePath);
		}

		try
		{
			context.ValidateDatabase();
			return new LibraryManager(context);
		}
		catch
		{
			context.Dispose();
			throw;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
		{
			return;
		}

		disposed = true;
		context.Dispose();
		GC.SuppressFinalize(this);
	}

	private JobsDbContext CreateReadContext()
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		return new JobsDbContext(databasePath);
	}

	private IJobRefreshService CreateRefreshService()
	{
		ConfigurationManager configurationManager =
			new ConfigurationManager();
		CookieManager cookieManager = new CookieManager();
		ScraperService service = new ScraperService(context);
		service.RegisterScraper(
			"LinkedIn",
			new LinkedInScraper(
				jobRepository,
				credentialRepository,
				cookieManager,
				configurationManager));
		service.RegisterScraper(
			"TokyoDev",
			new TokyoDevScraperSelenium(
				jobRepository,
				credentialRepository,
				cookieManager,
				configurationManager));
		return service;
	}
}
