namespace JobsDb.Core.Repositories
{
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
}
