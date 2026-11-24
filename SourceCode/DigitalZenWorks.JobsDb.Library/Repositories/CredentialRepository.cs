/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Stores scraper credentials in the JobsDb database.
/// </summary>
public class CredentialRepository : ICredentialRepository
{
	private readonly JobsDbContext context;

	/// <summary>
	/// Initializes a new instance of the <see cref="CredentialRepository"/>
	/// class.
	/// </summary>
	/// <param name="context">
	/// The database context supplied by the caller.
	/// </param>
	public CredentialRepository(JobsDbContext context)
	{
		this.context = context;
	}

	/// <inheritdoc/>
	public async Task<ScraperCredential> GetBySourceAsync(string source)
	{
		return await context.Credentials
			.FirstOrDefaultAsync(c => c.Source == source && c.IsActive).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<ScraperCredential> AddOrUpdateAsync(ScraperCredential credential)
	{
		var existing = await context.Credentials
			.FirstOrDefaultAsync(c => c.Source == credential.Source).ConfigureAwait(false);

		if (existing != null)
		{
			existing.Username = credential.Username;
			existing.EncryptedPassword = credential.EncryptedPassword;
			existing.CookieData = credential.CookieData;
			existing.IsActive = credential.IsActive;
			context.Credentials.Update(existing);
		}
		else
		{
			context.Credentials.Add(credential);
		}

		await context.SaveChangesAsync().ConfigureAwait(false);
		return existing ?? credential;
	}

	/// <inheritdoc/>
	public async Task<List<ScraperCredential>> GetAllActiveAsync()
	{
		return await context.Credentials
			.Where(c => c.IsActive)
			.ToListAsync().ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteBySourceAsync(string source)
	{
		var credential = await context.Credentials
			.FirstOrDefaultAsync(c => c.Source == source).ConfigureAwait(false);

		if (credential == null)
		{
			return false;
		}

		context.Credentials.Remove(credential);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return true;
	}
}
