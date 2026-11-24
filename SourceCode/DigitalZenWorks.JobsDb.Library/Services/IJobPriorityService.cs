/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobPriorityService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Saves the user's priority for a stored job.
/// </summary>
public interface IJobPriorityService
{
	/// <summary>
	/// Updates the priority for one job.
	/// </summary>
	/// <param name="jobId">The stored job identifier.</param>
	/// <param name="priority">The priority selected by the user.</param>
	/// <returns>True if a job was updated, otherwise false.</returns>
	Task<bool> SetPriorityAsync(int jobId, JobPriority priority);
}
