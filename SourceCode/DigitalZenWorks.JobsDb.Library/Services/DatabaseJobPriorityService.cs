/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseJobPriorityService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Updates priority without writing other job fields.
/// </summary>
internal sealed class DatabaseJobPriorityService : IJobPriorityService
{
	private readonly Func<JobsDbContext> createContext;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="DatabaseJobPriorityService"/> class.
	/// </summary>
	/// <param name="createContext">Creates a context for each write.</param>
	public DatabaseJobPriorityService(Func<JobsDbContext> createContext)
	{
		this.createContext = createContext;
	}

	/// <inheritdoc/>
	public async Task<bool> SetPriorityAsync(int jobId, JobPriority priority)
	{
		if (!Enum.IsDefined(priority))
		{
			throw new ArgumentOutOfRangeException(nameof(priority));
		}

		using JobsDbContext context = createContext();
		int updated = await context.Jobs
			.Where(job => job.Id == jobId)
			.ExecuteUpdateAsync(change => change.SetProperty(
				job => job.Priority, (int)priority)).ConfigureAwait(false);
		return updated != 0;
	}
}
