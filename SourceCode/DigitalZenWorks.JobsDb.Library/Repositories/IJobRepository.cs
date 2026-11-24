/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Provides job queries and explicit write operations.
/// </summary>
public interface IJobRepository : IJobReader
{
	/// <summary>
	/// Finds active jobs with the requested application status.
	/// </summary>
	/// <param name="status">The application status to match.</param>
	/// <returns>Matching jobs, newest posting first.</returns>
	Task<List<Job>> GetByStatusAsync(ApplicationStatus status);

	/// <summary>
	/// Looks up a listing by its database identifier.
	/// </summary>
	/// <param name="id">The database identifier of the job.</param>
	/// <returns>The matching listing, or null if none exists.</returns>
	Task<Job> GetByIdAsync(int id);

	/// <summary>
	/// Looks up a listing by its source identity.
	/// </summary>
	/// <param name="source">The source identifier.</param>
	/// <param name="sourceJobId">
	/// The posting identifier within the source.
	/// </param>
	/// <returns>The matching listing, or null if none exists.</returns>
	Task<Job> GetBySourceIdAsync(string source, string sourceJobId);

	/// <summary>
	/// Inserts a job and records its acquisition timestamp.
	/// </summary>
	/// <param name="job">The listing to insert.</param>
	/// <returns>The saved listing with its database identifier.</returns>
	Task<Job> AddAsync(Job job);

	/// <summary>
	/// Saves changes to an existing listing.
	/// </summary>
	/// <param name="job">The listing with the updated values.</param>
	/// <returns>The supplied listing after the changes are saved.</returns>
	Task<Job> UpdateAsync(Job job);

	/// <summary>
	/// Deletes a listing by database identifier.
	/// </summary>
	/// <param name="id">The database identifier of the job.</param>
	/// <returns>True if the listing was deleted, otherwise false.</returns>
	Task<bool> DeleteAsync(int id);

	/// <summary>
	/// Checks whether a specific source posting has been stored.
	/// </summary>
	/// <param name="source">The source identifier.</param>
	/// <param name="sourceJobId">
	/// The posting identifier within the source.
	/// </param>
	/// <returns>True if the listing exists, otherwise false.</returns>
	Task<bool> ExistsAsync(string source, string sourceJobId);

	/// <summary>
	/// Searches active listings by title, company, description, or location.
	/// </summary>
	/// <param name="searchTerm">
	/// The text to match, or blank to return all active listings.
	/// </param>
	/// <returns>Matching listings, newest posting first.</returns>
	Task<List<Job>> SearchAsync(string searchTerm);
}
