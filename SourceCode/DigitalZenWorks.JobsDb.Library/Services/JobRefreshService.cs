/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRefreshService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Owns each refresh session and prevents overlapping acquisition runs.
/// </summary>
internal sealed class JobRefreshService : IJobRefreshService
{
	private readonly Func<JobsDbContext> createContext;
	private readonly Func<JobsDbContext, IJobRefreshService> createService;
	private int refreshInProgress;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobRefreshService"/> class.
	/// </summary>
	/// <param name="createContext">
	/// Creates a database context for each refresh session.
	/// </param>
	/// <param name="createService">
	/// Creates the acquisition service for that context.
	/// </param>
	public JobRefreshService(
		Func<JobsDbContext> createContext,
		Func<JobsDbContext, IJobRefreshService> createService)
	{
		ArgumentNullException.ThrowIfNull(createContext);
		ArgumentNullException.ThrowIfNull(createService);
		this.createContext = createContext;
		this.createService = createService;
	}

	/// <inheritdoc/>
	public async Task<List<ScraperResult>> RefreshJobsAsync()
	{
		BeginRefresh();
		try
		{
			using JobsDbContext context = createContext();
			IJobRefreshService service = createService(context);
			return await service.RefreshJobsAsync().ConfigureAwait(false);
		}
		finally
		{
			Interlocked.Exchange(ref refreshInProgress, 0);
		}
	}

	/// <inheritdoc/>
	public async Task<ScraperResult> RefreshSourceAsync(string source)
	{
		BeginRefresh();
		try
		{
			using JobsDbContext context = createContext();
			IJobRefreshService service = createService(context);
			return await service.RefreshSourceAsync(source)
				.ConfigureAwait(false);
		}
		finally
		{
			Interlocked.Exchange(ref refreshInProgress, 0);
		}
	}

	private void BeginRefresh()
	{
		if (Interlocked.CompareExchange(ref refreshInProgress, 1, 0) != 0)
		{
			throw new InvalidOperationException(
				"A database refresh is already in progress.");
		}
	}
}
