/////////////////////////////////////////////////////////////////////////////
// <copyright file="ICredentialRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Defines storage operations for credentials used by job sources.
/// </summary>
public interface ICredentialRepository
{
	/// <summary>
	/// Looks up a credential by source name.
	/// </summary>
	/// <param name="source">The source identifier.</param>
	/// <returns>The matching credential, or null if none exists.</returns>
	Task<ScraperCredential> GetBySourceAsync(string source);

	/// <summary>
	/// Inserts or updates a credential using its source as the key.
	/// </summary>
	/// <param name="credential">The credential values to save.</param>
	/// <returns>The persisted credential.</returns>
	Task<ScraperCredential> AddOrUpdateAsync(ScraperCredential credential);

	/// <summary>
	/// Lists the credentials enabled for acquisition.
	/// </summary>
	/// <returns>The active credentials.</returns>
	Task<List<ScraperCredential>> GetAllActiveAsync();

	/// <summary>
	/// Deletes the credential for a source.
	/// </summary>
	/// <param name="source">The source identifier.</param>
	/// <returns>True if the credential was deleted, otherwise false.</returns>
	Task<bool> DeleteBySourceAsync(string source);
}
