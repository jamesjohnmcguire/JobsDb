/////////////////////////////////////////////////////////////////////////////
// <copyright file="LibraryManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
	private readonly IJobPriorityService jobPriorities;
	private readonly IJobApplicationService jobApplications;
	private readonly IEmailImportService emailImports;
	private readonly Lazy<IJobRefreshService> refreshService;
	private bool disposed;

	private LibraryManager(JobsDbContext context)
	{
		databasePath = Path.GetFullPath(context.GetDatabasePath());
		DatabaseJobReader jobReader =
			new DatabaseJobReader(CreateOperationContext);
		jobQueries = new JobQueryService(jobReader);
		jobPriorities = new DatabaseJobPriorityService(CreateOperationContext);
		jobApplications = new DatabaseJobApplicationService(
			CreateOperationContext);
		emailImports = new EmailImportService(
			CreateOperationContext, CreateEmailJobParser());
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

	/// <summary>Gets the service for saving job priorities.</summary>
	public IJobPriorityService JobPriorities
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return jobPriorities;
		}
	}

	/// <summary>Gets the service for recording applied marks.</summary>
	public IJobApplicationService JobApplications
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return jobApplications;
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
	/// Imports an email batch into the application database.
	/// </summary>
	/// <param name="directoryPath">The EML export directory.</param>
	/// <returns>The batch totals and individual message outcomes.</returns>
	public static Task<EmailImportBatchResult> ImportEmailsAsync(
		string directoryPath)
	{
		return ImportEmailsAsync(directoryPath, null);
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

	/// <summary>
	/// Imports an email batch using a supplied database path.
	/// </summary>
	/// <param name="directoryPath">The EML export directory.</param>
	/// <param name="databasePath">The database file or null.</param>
	/// <returns>The batch totals and individual message outcomes.</returns>
	internal static async Task<EmailImportBatchResult> ImportEmailsAsync(
		string directoryPath, string? databasePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
		using LibraryManager library = Initialize(databasePath);
		return await library.EmailImports.ImportDirectoryAsync(directoryPath)
			.ConfigureAwait(false);
	}

	private static EmailJobParserRouter CreateEmailJobParser()
	{
		LinkedInEmailParser linkedInParser = new LinkedInEmailParser();
		Dictionary<string, IEmailJobParser> parsersBySender =
			new Dictionary<string, IEmailJobParser>
			{
				{ LinkedInEmailParser.JobAlertSender, linkedInParser },
				{ IndeedEmailParser.JobAlertSender, new IndeedEmailParser() }
			};
		return new EmailJobParserRouter(parsersBySender);
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
