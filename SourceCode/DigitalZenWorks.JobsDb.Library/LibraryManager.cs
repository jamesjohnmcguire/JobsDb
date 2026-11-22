/////////////////////////////////////////////////////////////////////////////
// <copyright file="LibraryManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;

/// <summary>
/// Initializes library services and coordinates operation lifetimes.
/// </summary>
public sealed class LibraryManager : IDisposable
{
	private readonly string databasePath;
	private readonly IJobQueryService jobQueries;
	private readonly IEmailImportService emailImports;
	private readonly Lazy<IJobRefreshService> refreshService;
	private bool disposed;

	private LibraryManager(JobsDbContext context)
	{
		databasePath = Path.GetFullPath(context.GetDatabasePath());
		DatabaseJobReader jobReader =
			new DatabaseJobReader(CreateOperationContext);
		jobQueries = new JobQueryService(jobReader);
		emailImports = new EmailImportService(
			CreateOperationContext, new LinkedInEmailParser());
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

	/// <summary>Gets the exported-email import service.</summary>
	public IEmailImportService EmailImports
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return emailImports;
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

		using (context)
		{
			context.ValidateDatabase();
			return new LibraryManager(context);
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
		GC.SuppressFinalize(this);
	}

	private static IJobRefreshService CreateScraperService(
		JobsDbContext context)
	{
		IJobRepository jobRepository = new JobRepository(context);
		ICredentialRepository credentialRepository =
			new CredentialRepository(context);
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

	private JobsDbContext CreateOperationContext()
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		return new JobsDbContext(databasePath);
	}

	private IJobRefreshService CreateRefreshService()
	{
		return new JobRefreshService(
			CreateOperationContext, CreateScraperService);
	}
}
