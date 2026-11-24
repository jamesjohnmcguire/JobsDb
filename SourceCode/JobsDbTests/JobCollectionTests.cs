/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobCollectionTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using NUnit.Framework;

/// <summary>
/// Covers ordering, archival, and persistence of job query results.
/// </summary>
[TestFixture]
internal sealed class JobCollectionTests : BaseTestsSupport
{
	/// <summary>
	/// Prepares the database for job collection tests.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		DatabaseTestSchema.Create(TestDbPath!);
	}

	/// <summary>
	/// Removes the job collection test database.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		using JobsDbContext cleanupContext = new JobsDbContext(TestDbPath!);
		cleanupContext.Database.EnsureDeleted();
	}

	/// <summary>
	/// Checks that job collections are complete and ordered.
	/// </summary>
	/// <param name="includeArchived">
	/// Whether the collection includes archived listings.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false)]
	[TestCase(true)]
	public async Task GetJobsAsync_ReturnsOrderedCompleteCollection(
		bool includeArchived)
	{
		using JobsDbContext writeContext = new JobsDbContext(TestDbPath!);
		JobRepository repository = new JobRepository(writeContext);
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		Job olderJob = CreateJob("Older");
		olderJob.DatePosted = olderJob.DatePosted.AddDays(-1);
		Job archivedJob = CreateJob("Archived");
		archivedJob.IsArchived = true;
		Job newerJob = CreateJob("Newer");
		Job tiedJob = CreateJob("Tied");
		await repository.AddAsync(olderJob).ConfigureAwait(false);
		await repository.AddAsync(archivedJob).ConfigureAwait(false);
		await repository.AddAsync(newerJob).ConfigureAwait(false);
		await repository.AddAsync(tiedJob).ConfigureAwait(false);

		List<Job> jobs = await manager.JobQueries.GetJobsAsync(includeArchived)
			.ConfigureAwait(false);

		List<int> expectedIdentifiers = new List<int>
		{
			tiedJob.Id,
			newerJob.Id
		};
		if (includeArchived)
		{
			expectedIdentifiers.Add(archivedJob.Id);
		}

		expectedIdentifiers.Add(olderJob.Id);
		Assert.That(
			jobs.ConvertAll(job => job.Id), Is.EqualTo(expectedIdentifiers));
	}

	/// <summary>
	/// Checks that changing a returned job does not save it implicitly.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetJobsAsync_DisplayChangesRequireExplicitSave()
	{
		using JobsDbContext writeContext = new JobsDbContext(TestDbPath!);
		JobRepository repository = new JobRepository(writeContext);
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		Job originalJob = CreateJob("Original");
		await repository.AddAsync(originalJob).ConfigureAwait(false);
		List<Job> jobs =
			await manager.JobQueries.GetJobsAsync().ConfigureAwait(false);
		Job displayedJob = jobs[0];
		displayedJob.Title = "Edited in display";
		Job unrelatedJob = CreateJob("Unrelated");
		await repository.AddAsync(unrelatedJob).ConfigureAwait(false);

		List<Job> reloadedJobs =
			await manager.JobQueries.GetJobsAsync().ConfigureAwait(false);
		Job unchangedJob = reloadedJobs.Find(job => job.Id == originalJob.Id)!;
		Assert.That(unchangedJob.Title, Is.EqualTo("Original"));

		await repository.UpdateAsync(displayedJob).ConfigureAwait(false);
		using LibraryManager reader = LibraryManager.Initialize(TestDbPath);
		List<Job> savedJobs =
			await reader.JobQueries.GetJobsAsync().ConfigureAwait(false);
		Job savedJob = savedJobs.Find(job => job.Id == originalJob.Id)!;
		Assert.That(savedJob.Title, Is.EqualTo("Edited in display"));
	}

	/// <summary>
	/// Checks that an empty database yields an empty collection.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetJobsAsync_EmptyDatabase_ReturnsEmptyCollection()
	{
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		List<Job> jobs =
			await manager.JobQueries.GetJobsAsync().ConfigureAwait(false);

		Assert.That(jobs, Is.Empty);
	}

	private static Job CreateJob(string title)
	{
		return new Job
		{
			Title = title,
			Company = "Test Company",
			Source = "TestSource",
			SourceUrl = "https://example.com/jobs/" + title,
			SourceJobId = title,
			DatePosted = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		};
	}
}
