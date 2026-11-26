/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseJobApplicationService.cs" company="Digital Zen Works">
// Copyright c 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Saves an applied mark while preserving other application stages.
/// </summary>
internal sealed class DatabaseJobApplicationService : IJobApplicationService
{
	private readonly Func<JobsDbContext> createContext;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="DatabaseJobApplicationService"/> class.
	/// </summary>
	/// <param name="createContext">Creates a context for each write.</param>
	internal DatabaseJobApplicationService(Func<JobsDbContext> createContext)
	{
		this.createContext = createContext;
	}

	/// <inheritdoc/>
	public async Task<bool> SetAppliedAsync(int jobId, bool applied)
	{
		ApplicationStatus expected = ApplicationStatus.NotApplied;
		ApplicationStatus next = ApplicationStatus.Applied;
		if (!applied)
		{
			expected = ApplicationStatus.Applied;
			next = ApplicationStatus.NotApplied;
		}

		using JobsDbContext context = createContext();
		int updated = await context.Jobs
			.Where(job => job.Id == jobId && job.Status == expected)
			.ExecuteUpdateAsync(change => change.SetProperty(
				job => job.Status, next)).ConfigureAwait(false);
		return updated != 0;
	}
}
