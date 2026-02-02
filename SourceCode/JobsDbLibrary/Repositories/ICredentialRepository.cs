/////////////////////////////////////////////////////////////////////////////
// <copyright file="ICredentialRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using JobsDbLibrary.Scrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ICredentialRepository
{
	Task<ScraperCredential> GetBySourceAsync(string source);
	Task<ScraperCredential> AddOrUpdateAsync(ScraperCredential credential);
	Task<List<ScraperCredential>> GetAllActiveAsync();
	Task<bool> DeleteBySourceAsync(string source);
}

