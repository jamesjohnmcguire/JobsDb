/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseJobReader.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Owns a separate database context for each collection read.
/// </summary>
internal sealed class DatabaseJobReader : IJobReader
{
	private readonly Func<JobsDbContext> createContext;

	public DatabaseJobReader(Func<JobsDbContext> createContext)
	{
		ArgumentNullException.ThrowIfNull(createContext);
		this.createContext = createContext;
	}

	/// <inheritdoc/>
	public async Task<List<Job>> GetAllAsync()
	{
		using JobsDbContext context = createContext();
		JobRepository repository = new JobRepository(context);
		return await repository.GetAllAsync().ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<List<Job>> GetActiveJobsAsync()
	{
		using JobsDbContext context = createContext();
		JobRepository repository = new JobRepository(context);
		return await repository.GetActiveJobsAsync().ConfigureAwait(false);
	}
}
