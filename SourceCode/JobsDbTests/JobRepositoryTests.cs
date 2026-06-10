/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRepositoryTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Repositories;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
internal sealed class JobRepositoryTests : BaseTestsSupport
{
	private JobsDbContext _context;
	private JobRepository _repository;
	private string _testDbPath;

	[SetUp]
	public void SetUp()
	{
		_testDbPath = Path.Combine(Path.GetTempPath(), $"test_jobs_{Guid.NewGuid()}.db");
		_context = new JobsDbContext(_testDbPath);
		_context.Database.EnsureCreated();
		_repository = new JobRepository(_context);
	}

	[TearDown]
	public void TearDown()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();

		if (File.Exists(_testDbPath))
		{
			File.Delete(_testDbPath);
		}
	}

	[Test]
	public async Task AddAsync_ValidJob_AddsToDatabase()
	{
		var result = await _repository.AddAsync(TestJob).ConfigureAwait(false);

		Assert.That(result.Id, Is.GreaterThan(0));
		Assert.That(result.DateScraped, Is.Not.EqualTo(default(DateTime)));

		var jobs = await _repository.GetAllAsync().ConfigureAwait(false);
		Assert.That(jobs.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task GetByIdAsync_ExistingJob_ReturnsJob()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var result = await _repository.GetByIdAsync(job.Id).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(job.Id));
		Assert.That(result.Title, Is.EqualTo(job.Title));
	}

	[Test]
	public async Task GetByIdAsync_NonExistingJob_ReturnsNull()
	{
		// Act
		var result = await _repository.GetByIdAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task GetBySourceIdAsync_ExistingJob_ReturnsJob()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var result = await _repository.GetBySourceIdAsync(job.Source, job.SourceJobId).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.SourceJobId, Is.EqualTo(job.SourceJobId));
	}

	[Test]
	public async Task UpdateAsync_ExistingJob_UpdatesDatabase()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		job.Title = "Updated Title";
		job.Status = ApplicationStatus.Applied;

		// Act
		var result = await _repository.UpdateAsync(job).ConfigureAwait(false);

		// Assert
		var updated = await _repository.GetByIdAsync(job.Id).ConfigureAwait(false);
		Assert.That(updated.Title, Is.EqualTo("Updated Title"));
		Assert.That(updated.Status, Is.EqualTo(ApplicationStatus.Applied));
	}

	[Test]
	public async Task DeleteAsync_ExistingJob_RemovesFromDatabase()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		var result = await _repository.DeleteAsync(job.Id).ConfigureAwait(false);

		Assert.That(result, Is.True);
		var deleted = await _repository.GetByIdAsync(job.Id).ConfigureAwait(false);
		Assert.That(deleted, Is.Null);
	}

	[Test]
	public async Task DeleteAsync_NonExistingJob_ReturnsFalse()
	{
		// Act
		var result = await _repository.DeleteAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public async Task GetByStatusAsync_FiltersByStatus_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Status = ApplicationStatus.NotApplied;
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Source = "AnotherSource";
		tempJob2.Status = ApplicationStatus.Applied;
		Job job2 = await _repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Source = "AnotherSourceAgain";
		tempJob3.Status = ApplicationStatus.Applied;
		Job job3 = await _repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var appliedJobs = await _repository.GetByStatusAsync(ApplicationStatus.Applied).ConfigureAwait(false);

		// Assert
		Assert.That(appliedJobs.Count, Is.EqualTo(2));
		Assert.That(appliedJobs.All(j => j.Status == ApplicationStatus.Applied), Is.True);
	}

	[Test]
	public async Task GetActiveJobsAsync_ExcludesArchived_ReturnsOnlyActive()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.IsArchived = false;
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.IsArchived = false;
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.IsArchived = true;
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var activeJobs = await _repository.GetActiveJobsAsync().ConfigureAwait(false);

		// Assert
		Assert.That(activeJobs.Count, Is.EqualTo(2));
		Assert.That(activeJobs.All(j => !j.IsArchived), Is.True);
	}

	[Test]
	public async Task SearchAsync_ByTitle_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Title = "Senior Software Engineer";
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Title = "Junior Developer";
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Title = "Senior DevOps Engineer";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var results = await _repository.SearchAsync("Senior").ConfigureAwait(false);

		// Assert
		Assert.That(results.Count, Is.EqualTo(2));
		Assert.That(results.All(j => j.Title.Contains("Senior", StringComparison.InvariantCultureIgnoreCase)), Is.True);
	}

	[Test]
	public async Task SearchAsync_ByCompany_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Company = "Google";
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Company = "Microsoft";
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Company = "Amazon";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var results = await _repository.SearchAsync("Microsoft").ConfigureAwait(false);

		// Assert
		Assert.That(results.Count, Is.EqualTo(1));
		Assert.That(results[0].Company, Is.EqualTo("Microsoft"));
	}

	[Test]
	public async Task ExistsAsync_ExistingJob_ReturnsTrue()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var exists = await _repository.ExistsAsync(job.Source, job.SourceJobId).ConfigureAwait(false);

		// Assert
		Assert.That(exists, Is.True);
	}

	[Test]
	public async Task ExistsAsync_NonExistingJob_ReturnsFalse()
	{
		// Act
		var exists = await _repository.ExistsAsync("LinkedIn", "nonexistent").ConfigureAwait(false);

		// Assert
		Assert.That(exists, Is.False);
	}

	private static Job CreateTestJob(
		string title = "Test Job",
		string company = "Test Company",
		ApplicationStatus status = ApplicationStatus.NotApplied,
		bool isArchived = false)
	{
		return new Job
		{
			Title = title,
			Company = company,
			Location = "Tokyo, Japan",
			Source = "LinkedIn",
			SourceUrl = $"https://linkedin.com/jobs/{Guid.NewGuid()}",
			SourceJobId = Guid.NewGuid().ToString(),
			DatePosted = DateTime.UtcNow,
			Status = status,
			IsArchived = isArchived,
			Description = "Test job description",
			Requirements = "Test requirements"
		};
	}
}
