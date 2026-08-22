/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilterRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;

public class SearchFilterRepository : ISearchFilterRepository
{
	private readonly JobsDbContext context;

	public SearchFilterRepository(JobsDbContext context)
	{
		this.context = context;
	}

	public async Task<List<SearchFilter>> GetAllAsync()
	{
		return await context.SearchFilters
			.OrderBy(f => f.Name)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<List<SearchFilter>> GetActiveAsync()
	{
		return await context.SearchFilters
			.Where(f => f.IsActive)
			.OrderBy(f => f.Name)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<SearchFilter> GetByIdAsync(int id)
	{
		return await context.SearchFilters.FindAsync(id).ConfigureAwait(false);
	}

	public async Task<SearchFilter> AddAsync(SearchFilter filter)
	{
		filter.CreatedDate = DateTime.UtcNow;
		context.SearchFilters.Add(filter);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return filter;
	}

	public async Task<SearchFilter> UpdateAsync(SearchFilter filter)
	{
		context.SearchFilters.Update(filter);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return filter;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var filter = await GetByIdAsync(id).ConfigureAwait(false);
		if (filter == null)
		{
			return false;
		}

		context.SearchFilters.Remove(filter);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return true;
	}
}
