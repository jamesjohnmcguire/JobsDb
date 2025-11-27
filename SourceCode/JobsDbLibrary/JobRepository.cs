using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsDb.Core.Repositories
{
	public class JobRepository : IJobRepository
	{
		private readonly JobsDbContext _context;

		public JobRepository(JobsDbContext context)
		{
			_context = context;
		}

		public async Task<List<Job>> GetAllAsync()
		{
			return await _context.Jobs
				.OrderByDescending(j => j.DatePosted)
				.ToListAsync();
		}

		public async Task<List<Job>> GetByStatusAsync(ApplicationStatus status)
		{
			return await _context.Jobs
				.Where(j => j.Status == status && !j.IsArchived)
				.OrderByDescending(j => j.DatePosted)
				.ToListAsync();
		}

		public async Task<List<Job>> GetActiveJobsAsync()
		{
			return await _context.Jobs
				.Where(j => !j.IsArchived)
				.OrderByDescending(j => j.DatePosted)
				.ToListAsync();
		}

		public async Task<Job> GetByIdAsync(int id)
		{
			return await _context.Jobs.FindAsync(id);
		}

		public async Task<Job> GetBySourceIdAsync(string source, string sourceJobId)
		{
			return await _context.Jobs
				.FirstOrDefaultAsync(j => j.Source == source && j.SourceJobId == sourceJobId);
		}

		public async Task<Job> AddAsync(Job job)
		{
			job.DateScraped = DateTime.UtcNow;
			_context.Jobs.Add(job);
			await _context.SaveChangesAsync();
			return job;
		}

		public async Task<Job> UpdateAsync(Job job)
		{
			_context.Jobs.Update(job);
			await _context.SaveChangesAsync();
			return job;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var job = await GetByIdAsync(id);
			if (job == null) return false;

			_context.Jobs.Remove(job);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> ExistsAsync(string source, string sourceJobId)
		{
			return await _context.Jobs
				.AnyAsync(j => j.Source == source && j.SourceJobId == sourceJobId);
		}

		public async Task<List<Job>> SearchAsync(string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
				return await GetActiveJobsAsync();

			var term = searchTerm.ToLower();
			return await _context.Jobs
				.Where(j => !j.IsArchived && (
					j.Title.ToLower().Contains(term) ||
					j.Company.ToLower().Contains(term) ||
					j.Description.ToLower().Contains(term) ||
					j.Location.ToLower().Contains(term)
				))
				.OrderByDescending(j => j.DatePosted)
				.ToListAsync();
		}
	}
}
