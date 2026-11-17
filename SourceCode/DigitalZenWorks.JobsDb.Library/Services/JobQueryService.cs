/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobQueryService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;

/// <summary>
/// Selects the job collection requested by an application.
/// </summary>
internal sealed class JobQueryService : IJobQueryService
{
	private readonly IJobRepository jobRepository;

	public JobQueryService(IJobRepository jobRepository)
	{
		ArgumentNullException.ThrowIfNull(jobRepository);
		this.jobRepository = jobRepository;
	}

	/// <inheritdoc/>
	public Task<List<Job>> GetJobsAsync(bool includeArchived = false)
	{
		if (includeArchived)
		{
			return jobRepository.GetAllAsync();
		}

		return jobRepository.GetActiveJobsAsync();
	}
}
