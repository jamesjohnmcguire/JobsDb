/////////////////////////////////////////////////////////////////////////////
// <copyright file="ISearchFilterRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using JobsDb.Core.Scrapers;

public interface ISearchFilterRepository
{
	Task<List<SearchFilter>> GetAllAsync();

	Task<List<SearchFilter>> GetActiveAsync();

	Task<SearchFilter> GetByIdAsync(int id);

	Task<SearchFilter> AddAsync(SearchFilter filter);

	Task<SearchFilter> UpdateAsync(SearchFilter filter);

	Task<bool> DeleteAsync(int id);
}
