/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using JobsDb.Core.Scrapers;
using JobsDbLibrary.Scrapers;
using Microsoft.EntityFrameworkCore;

public class CredentialRepository : ICredentialRepository
{
	private readonly JobsDbContext _context;

	public CredentialRepository(JobsDbContext context)
	{
		_context = context;
	}

	public async Task<ScraperCredential> GetBySourceAsync(string source)
	{
		return await _context.Credentials
			.FirstOrDefaultAsync(c => c.Source == source && c.IsActive).ConfigureAwait(false);
	}

	public async Task<ScraperCredential> AddOrUpdateAsync(ScraperCredential credential)
	{
		var existing = await _context.Credentials
			.FirstOrDefaultAsync(c => c.Source == credential.Source).ConfigureAwait(false);

		if (existing != null)
		{
			existing.Username = credential.Username;
			existing.EncryptedPassword = credential.EncryptedPassword;
			existing.CookieData = credential.CookieData;
			existing.IsActive = credential.IsActive;
			_context.Credentials.Update(existing);
		}
		else
		{
			_context.Credentials.Add(credential);
		}

		await _context.SaveChangesAsync().ConfigureAwait(false);
		return existing ?? credential;
	}

	public async Task<List<ScraperCredential>> GetAllActiveAsync()
	{
		return await _context.Credentials
			.Where(c => c.IsActive)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<bool> DeleteBySourceAsync(string source)
	{
		var credential = await _context.Credentials
			.FirstOrDefaultAsync(c => c.Source == source).ConfigureAwait(false);

		if (credential == null)
		{
			return false;
		}

		_context.Credentials.Remove(credential);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return true;
	}
}
