/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobApplicationService.cs" company="Digital Zen Works">
// Copyright c 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Services;

using System.Threading.Tasks;

/// <summary>
/// Records whether the user has applied to a posting.
/// </summary>
public interface IJobApplicationService
{
	/// <summary>
	/// Marks an application or removes a basic applied mark.
	/// </summary>
	/// <param name="jobId">The stored job identifier.</param>
	/// <param name="applied">Whether the user applied.</param>
	/// <returns>True if the expected status was found and updated.</returns>
	Task<bool> SetAppliedAsync(int jobId, bool applied);
}
