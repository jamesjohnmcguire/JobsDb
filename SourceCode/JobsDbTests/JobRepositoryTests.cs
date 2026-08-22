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
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
internal sealed class JobRepositoryTests : BaseTestsSupport
{
	private JobsDbContext context;
	private JobRepository repository;
	private string testDbPath;

	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_jobs_{Guid.NewGuid()}.db");
		context = new JobsDbContext(testDbPath);
		context.Database.EnsureCreated();
		repository = new JobRepository(context);
	}

	[TearDown]
	public void TearDown()
	{
		context.Database.EnsureDeleted();
		context.Dispose();

		if (File.Exists(testDbPath))
		{
			File.Delete(testDbPath);
		}
	}

	[Test]
	public async Task AddAsync_ValidJob_AddsToDatabase()
	{
		var result = await repository.AddAsync(TestJob).ConfigureAwait(false);

		Assert.That(result.Id, Is.GreaterThan(0));
		Assert.That(result.DateScraped, Is.Not.EqualTo(default(DateTime)));

		var jobs = await repository.GetAllAsync().ConfigureAwait(false);
		Assert.That(jobs.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task GetByIdAsync_ExistingJob_ReturnsJob()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var result = await repository.GetByIdAsync(job.Id).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(job.Id));
		Assert.That(result.Title, Is.EqualTo(job.Title));
	}

	[Test]
	public async Task GetByIdAsync_NonExistingJob_ReturnsNull()
	{
		// Act
		var result = await repository.GetByIdAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task GetBySourceIdAsync_ExistingJob_ReturnsJob()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var result = await repository.GetBySourceIdAsync(job.Source, job.SourceJobId).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.SourceJobId, Is.EqualTo(job.SourceJobId));
	}

	[Test]
	public async Task UpdateAsync_ExistingJob_UpdatesDatabase()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		job.Title = "Updated Title";
		job.Status = ApplicationStatus.Applied;

		// Act
		var result = await repository.UpdateAsync(job).ConfigureAwait(false);

		// Assert
		var updated = await repository.GetByIdAsync(job.Id).ConfigureAwait(false);
		Assert.That(updated.Title, Is.EqualTo("Updated Title"));
		Assert.That(updated.Status, Is.EqualTo(ApplicationStatus.Applied));
	}

	[Test]
	public async Task DeleteAsync_ExistingJob_RemovesFromDatabase()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		var result = await repository.DeleteAsync(job.Id).ConfigureAwait(false);

		Assert.That(result, Is.True);
		var deleted = await repository.GetByIdAsync(job.Id).ConfigureAwait(false);
		Assert.That(deleted, Is.Null);
	}

	[Test]
	public async Task DeleteAsync_NonExistingJob_ReturnsFalse()
	{
		// Act
		var result = await repository.DeleteAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public async Task GetByStatusAsync_FiltersByStatus_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Status = ApplicationStatus.NotApplied;
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Source = "AnotherSource";
		tempJob2.Status = ApplicationStatus.Applied;
		Job job2 = await repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Source = "AnotherSourceAgain";
		tempJob3.Status = ApplicationStatus.Applied;
		Job job3 = await repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var appliedJobs = await repository.GetByStatusAsync(ApplicationStatus.Applied).ConfigureAwait(false);

		// Assert
		Assert.That(appliedJobs.Count, Is.EqualTo(2));
		Assert.That(appliedJobs.All(j => j.Status == ApplicationStatus.Applied), Is.True);
	}

	[Test]
	public async Task GetActiveJobsAsync_ExcludesArchived_ReturnsOnlyActive()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.IsArchived = false;
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.IsArchived = false;
		tempJob2.Source = "AnotherSource";
		Job job2 = await repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.IsArchived = true;
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var activeJobs = await repository.GetActiveJobsAsync().ConfigureAwait(false);

		// Assert
		Assert.That(activeJobs.Count, Is.EqualTo(2));
		Assert.That(activeJobs.All(j => !j.IsArchived), Is.True);
	}

	[Test]
	public async Task SearchAsync_ByTitle_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Title = "Senior Software Engineer";
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Title = "Junior Developer";
		tempJob2.Source = "AnotherSource";
		Job job2 = await repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Title = "Senior DevOps Engineer";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var results = await repository.SearchAsync("Senior").ConfigureAwait(false);

		// Assert
		Assert.That(results.Count, Is.EqualTo(2));
		Assert.That(results.All(j => j.Title.Contains("Senior", StringComparison.InvariantCultureIgnoreCase)), Is.True);
	}

	[Test]
	public async Task SearchAsync_ByCompany_ReturnsMatchingJobs()
	{
		Job tempJob = CopyJob(TestJob);
		tempJob.Company = "Google";
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Company = "Microsoft";
		tempJob2.Source = "AnotherSource";
		Job job2 = await repository.AddAsync(tempJob2).ConfigureAwait(false);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Company = "Amazon";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await repository.AddAsync(tempJob3).ConfigureAwait(false);

		// Act
		var results = await repository.SearchAsync("Microsoft").ConfigureAwait(false);

		// Assert
		Assert.That(results.Count, Is.EqualTo(1));
		Assert.That(results[0].Company, Is.EqualTo("Microsoft"));
	}

	[Test]
	public async Task ExistsAsync_ExistingJob_ReturnsTrue()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await repository.AddAsync(tempJob).ConfigureAwait(false);

		// Act
		var exists = await repository.ExistsAsync(job.Source, job.SourceJobId).ConfigureAwait(false);

		// Assert
		Assert.That(exists, Is.True);
	}

	[Test]
	public async Task ExistsAsync_NonExistingJob_ReturnsFalse()
	{
		// Act
		var exists = await repository.ExistsAsync("LinkedIn", "nonexistent").ConfigureAwait(false);

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
