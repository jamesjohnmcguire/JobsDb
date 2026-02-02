namespace JobsDb.Core.Repositories
{
	using JobsDb.Core.Scrapers;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public interface ISearchFilterRepository
	{
		Task<List<SearchFilter>> GetAllAsync();
		Task<List<SearchFilter>> GetActiveAsync();
		Task<SearchFilter> GetByIdAsync(int id);
		Task<SearchFilter> AddAsync(SearchFilter filter);
		Task<SearchFilter> UpdateAsync(SearchFilter filter);
		Task<bool> DeleteAsync(int id);
	}
}
