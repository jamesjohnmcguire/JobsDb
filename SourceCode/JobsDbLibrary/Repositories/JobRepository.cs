/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRepository.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using Microsoft.EntityFrameworkCore;

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
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<List<Job>> GetByStatusAsync(ApplicationStatus status)
	{
		return await _context.Jobs
			.Where(j => j.Status == status && !j.IsArchived)
			.OrderByDescending(j => j.DatePosted)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<List<Job>> GetActiveJobsAsync()
	{
		return await _context.Jobs
			.Where(j => !j.IsArchived)
			.OrderByDescending(j => j.DatePosted)
			.ToListAsync().ConfigureAwait(false);
	}

	public async Task<Job> GetByIdAsync(int id)
	{
		return await _context.Jobs.FindAsync(id).ConfigureAwait(false);
	}

	public async Task<Job> GetBySourceIdAsync(string source, string sourceJobId)
	{
		return await _context.Jobs
			.FirstOrDefaultAsync(j => j.Source == source && j.SourceJobId == sourceJobId).ConfigureAwait(false);
	}

	public async Task<Job> AddAsync(Job job)
	{
		job.DateScraped = DateTime.UtcNow;
		_context.Jobs.Add(job);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return job;
	}

	public async Task<Job> UpdateAsync(Job job)
	{
		_context.Jobs.Update(job);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return job;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var job = await GetByIdAsync(id).ConfigureAwait(false);
		if (job == null) return false;

		_context.Jobs.Remove(job);
		await _context.SaveChangesAsync().ConfigureAwait(false);
		return true;
	}

	public async Task<bool> ExistsAsync(string source, string sourceJobId)
	{
		return await _context.Jobs
			.AnyAsync(j => j.Source == source && j.SourceJobId == sourceJobId).ConfigureAwait(false);
	}

	public async Task<List<Job>> SearchAsync(string searchTerm)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
			return await GetActiveJobsAsync().ConfigureAwait(false);

		var term = searchTerm.ToLower(CultureInfo.InvariantCulture);

		var results = await _context.Jobs
			.Where(j => !j.IsArchived &&
				(EF.Functions.Like(j.Title, $"%{term}%") ||
				 EF.Functions.Like(j.Company, $"%{term}%") ||
				 EF.Functions.Like(j.Description, $"%{term}%") ||
				 EF.Functions.Like(j.Location, $"%{term}%")))
			.OrderByDescending(j => j.DatePosted)
			.ToListAsync().ConfigureAwait(false);

		return results;
	}
}
