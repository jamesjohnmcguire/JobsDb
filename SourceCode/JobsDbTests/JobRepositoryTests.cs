/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRepositoryTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using NUnit.Framework;

/// <summary>
/// Covers job persistence and query behavior against SQLite.
/// </summary>
[TestFixture]
internal sealed class JobRepositoryTests : BaseTestsSupport
{
	private JobsDbContext context;
	private JobRepository repository;
	private string testDbPath;

	/// <summary>
	/// Prepares a database and job repository for the test.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_jobs_{Guid.NewGuid()}.db");
		DatabaseTestSchema.Create(testDbPath);
		context = new JobsDbContext(testDbPath);
		repository = new JobRepository(context);
	}

	/// <summary>
	/// Disposes the context and removes the temporary database.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		try
		{
			context?.Database.EnsureDeleted();
		}
		finally
		{
			context?.Dispose();
		}

		if (File.Exists(testDbPath))
		{
			File.Delete(testDbPath);
		}
	}

	/// <summary>
	/// Checks that insertion assigns an identifier and acquisition timestamp.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task AddAsync_ValidJob_AddsToDatabase()
	{
		var result = await repository.AddAsync(TestJob).ConfigureAwait(false);

		Assert.That(result.Id, Is.GreaterThan(0));
		Assert.That(result.DateScraped, Is.Not.EqualTo(default(DateTime)));

		var jobs = await repository.GetAllAsync().ConfigureAwait(false);
		Assert.That(jobs.Count, Is.EqualTo(1));
	}

	/// <summary>
	/// Checks that a saved job can be found by its database identifier.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that lookup of an unknown identifier returns null.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetByIdAsync_NonExistingJob_ReturnsNull()
	{
		// Act
		var result = await repository.GetByIdAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

	/// <summary>
	/// Checks that a job can be found using its source and posting identifier.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that changing a job persists the updated values.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that deleting an existing job removes it from the database.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that deleting an unknown identifier returns false.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task DeleteAsync_NonExistingJob_ReturnsFalse()
	{
		// Act
		var result = await repository.DeleteAsync(999).ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.False);
	}

	/// <summary>
	/// Checks that a status query returns only matching jobs.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that the active query excludes archived jobs.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that search matches terms in a job title.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that search matches terms in a company name.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that existence lookup recognizes a stored source posting.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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

	/// <summary>
	/// Checks that existence lookup returns false for an unknown posting.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
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
