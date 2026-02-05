/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilterRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Scrapers;
using Microsoft.EntityFrameworkCore;

public class SearchFilterRepository : ISearchFilterRepository
{
	private readonly JobsDbContext _context;

	public SearchFilterRepository(JobsDbContext context)
	{
		_context = context;
	}

	public async Task<List<SearchFilter>> GetAllAsync()
	{
		return await _context.SearchFilters
			.OrderBy(f => f.Name)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<List<SearchFilter>> GetActiveAsync()
	{
		return await _context.SearchFilters
			.Where(f => f.IsActive)
			.OrderBy(f => f.Name)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<SearchFilter> GetByIdAsync(int id)
	{
		return await _context.SearchFilters.FindAsync(id).ConfigureAwait(false);
	}

	public async Task<SearchFilter> AddAsync(SearchFilter filter)
	{
		filter.CreatedDate = DateTime.UtcNow;
		_context.SearchFilters.Add(filter);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return filter;
	}

	public async Task<SearchFilter> UpdateAsync(SearchFilter filter)
	{
		_context.SearchFilters.Update(filter);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return filter;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var filter = await GetByIdAsync(id).ConfigureAwait(false);
		if (filter == null) return false;

		_context.SearchFilters.Remove(filter);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return true;
	}
}
