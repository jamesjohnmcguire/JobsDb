/////////////////////////////////////////////////////////////////////////////
// <copyright file="LibraryManagerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Services;
using NUnit.Framework;

[TestFixture]
internal sealed class LibraryManagerTests : BaseTestsSupport
{
	[SetUp]
	public void SetUp()
	{
		DatabaseTestSchema.Create(TestDbPath!);
	}

	[TearDown]
	public void TearDown()
	{
		using JobsDbContext cleanupContext = new JobsDbContext(TestDbPath!);
		cleanupContext.Database.EnsureDeleted();
	}

	[Test]
	public async Task Initialize_PreparedDatabase_ProvidesPersistentJobs()
	{
		using (JobsDbContext preparation = new JobsDbContext(TestDbPath!))
		{
			Job job = new Job
			{
				Title = "Software Developer",
				Company = "Test Company",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/1",
				SourceJobId = "1"
			};
			JobRepository repository = new JobRepository(preparation);
			await repository.AddAsync(job).ConfigureAwait(false);
		}

		using LibraryManager reader = LibraryManager.Initialize(TestDbPath);
		List<Job> jobs =
			await reader.JobQueries.GetJobsAsync().ConfigureAwait(false);
		Assert.That(jobs, Has.Count.EqualTo(1));
		Assert.That(jobs[0].Title, Is.EqualTo("Software Developer"));
	}

	[Test]
	public void Initialize_MissingDatabase_DoesNotCreateIt()
	{
		string databasePath = TestDbPath + ".missing.db";
		InvalidOperationException? exception =
			Assert.Throws<InvalidOperationException>(
				() => LibraryManager.Initialize(databasePath));

		Assert.That(exception!.Message, Does.Contain("updateDatabase"));
		Assert.That(File.Exists(databasePath), Is.False);
	}

	[Test]
	public void Initialize_InvalidDatabase_CanRetryAfterPreparation()
	{
		using (JobsDbContext cleanupContext = new JobsDbContext(TestDbPath!))
		{
			cleanupContext.Database.EnsureDeleted();
		}

		using (FileStream emptyDatabase = File.Create(TestDbPath!))
		{
		}

		Assert.Throws<InvalidOperationException>(
			() => LibraryManager.Initialize(TestDbPath));
		DatabaseTestSchema.Create(TestDbPath!);
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		Assert.That(manager.JobQueries, Is.InstanceOf<IJobQueryService>());
	}

	[Test]
	public void Dispose_PreventsFurtherServiceAccess()
	{
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		IJobQueryService queries = manager.JobQueries;
		IJobRefreshService refresh = manager.Refresh;
		manager.Dispose();
		manager.Dispose();

		Assert.Throws<ObjectDisposedException>(() => _ = manager.JobQueries);
		Assert.Throws<ObjectDisposedException>(() => _ = manager.Refresh);
		Assert.Throws<ObjectDisposedException>(() => _ = manager.EmailImports);
		Assert.ThrowsAsync<ObjectDisposedException>(
			async () => await queries.GetJobsAsync().ConfigureAwait(false));
		Assert.ThrowsAsync<ObjectDisposedException>(
			async () => await refresh.RefreshJobsAsync().ConfigureAwait(false));
	}
}
