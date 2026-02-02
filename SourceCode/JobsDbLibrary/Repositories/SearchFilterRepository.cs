namespace JobsDb.Core.Repositories
{
	using JobsDb.Core.Scrapers;
	using JobsDb.Core.Data;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.EntityFrameworkCore;
	using System;

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
				.ToListAsync();
		}

		public async Task<List<SearchFilter>> GetActiveAsync()
		{
			return await _context.SearchFilters
				.Where(f => f.IsActive)
				.OrderBy(f => f.Name)
				.ToListAsync();
		}

		public async Task<SearchFilter> GetByIdAsync(int id)
		{
			return await _context.SearchFilters.FindAsync(id);
		}

		public async Task<SearchFilter> AddAsync(SearchFilter filter)
		{
			filter.CreatedDate = DateTime.UtcNow;
			_context.SearchFilters.Add(filter);
			await _context.SaveChangesAsync();
			return filter;
		}

		public async Task<SearchFilter> UpdateAsync(SearchFilter filter)
		{
			_context.SearchFilters.Update(filter);
			await _context.SaveChangesAsync();
			return filter;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var filter = await GetByIdAsync(id);
			if (filter == null) return false;

			_context.SearchFilters.Remove(filter);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
