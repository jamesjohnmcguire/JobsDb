/////////////////////////////////////////////////////////////////////////////
// <copyright file="ISearchFilterRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Defines storage operations for saved search filters.
/// </summary>
public interface ISearchFilterRepository
{
	/// <summary>
	/// Lists all saved search filters.
	/// </summary>
	/// <returns>The saved filters.</returns>
	Task<List<SearchFilter>> GetAllAsync();

	/// <summary>
	/// Lists search filters enabled for use.
	/// </summary>
	/// <returns>The active filters.</returns>
	Task<List<SearchFilter>> GetActiveAsync();

	/// <summary>
	/// Looks up a saved search filter by identifier.
	/// </summary>
	/// <param name="id">The database identifier of the filter.</param>
	/// <returns>The matching filter, or null if none exists.</returns>
	Task<SearchFilter> GetByIdAsync(int id);

	/// <summary>
	/// Creates a saved search filter.
	/// </summary>
	/// <param name="filter">The filter to insert.</param>
	/// <returns>The saved filter with its database identifier.</returns>
	Task<SearchFilter> AddAsync(SearchFilter filter);

	/// <summary>
	/// Saves changes to a search filter.
	/// </summary>
	/// <param name="filter">The filter with the updated values.</param>
	/// <returns>The saved filter.</returns>
	Task<SearchFilter> UpdateAsync(SearchFilter filter);

	/// <summary>
	/// Deletes a saved search filter.
	/// </summary>
	/// <param name="id">The database identifier of the filter.</param>
	/// <returns>True if the filter was deleted, otherwise false.</returns>
	Task<bool> DeleteAsync(int id);
}
