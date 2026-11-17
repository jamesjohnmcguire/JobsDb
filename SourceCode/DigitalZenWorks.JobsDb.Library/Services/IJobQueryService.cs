/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobQueryService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Supplies job collections for clients to display.
/// </summary>
public interface IJobQueryService
{
	/// <summary>
	/// Gets a complete collection, newest postings first.
	/// Returned jobs are detached; changes require an explicit write.
	/// Clients handle pagination and display formatting.
	/// </summary>
	/// <param name="includeArchived">Whether to include archived jobs.</param>
	/// <returns>The requested job collection.</returns>
	Task<List<Job>> GetJobsAsync(bool includeArchived = false);
}
