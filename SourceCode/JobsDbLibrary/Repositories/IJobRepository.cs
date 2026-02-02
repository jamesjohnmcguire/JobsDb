/////////////////////////////////////////////////////////////////////////////
// <copyright file="IJobRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using JobsDb.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IJobRepository
{
	Task<List<Job>> GetAllAsync();
	Task<List<Job>> GetByStatusAsync(ApplicationStatus status);
	Task<List<Job>> GetActiveJobsAsync();
	Task<Job> GetByIdAsync(int id);
	Task<Job> GetBySourceIdAsync(string source, string sourceJobId);
	Task<Job> AddAsync(Job job);
	Task<Job> UpdateAsync(Job job);
	Task<bool> DeleteAsync(int id);
	Task<bool> ExistsAsync(string source, string sourceJobId);
	Task<List<Job>> SearchAsync(string searchTerm);
}
