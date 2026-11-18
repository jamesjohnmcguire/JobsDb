/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobReader.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Reads complete, detached job collections in posting order.
/// </summary>
public interface IJobReader
{
	/// <summary>
	/// Reads all jobs, including archived jobs.
	/// </summary>
	/// <returns>The job collection.</returns>
	Task<List<Job>> GetAllAsync();

	/// <summary>
	/// Reads jobs that have not been archived.
	/// </summary>
	/// <returns>The active job collection.</returns>
	Task<List<Job>> GetActiveJobsAsync();
}
