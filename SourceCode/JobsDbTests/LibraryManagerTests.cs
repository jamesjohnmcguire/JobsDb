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
		using (LibraryManager manager = LibraryManager.Initialize(TestDbPath))
		{
			Job job = new Job
			{
				Title = "Software Developer",
				Company = "Test Company",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/1",
				SourceJobId = "1"
			};
			await manager.Jobs.AddAsync(job).ConfigureAwait(false);
		}

		using LibraryManager reader = LibraryManager.Initialize(TestDbPath);
		List<Job> jobs =
			await reader.JobQueries.GetJobsAsync().ConfigureAwait(false);
		Assert.That(jobs, Has.Count.EqualTo(1));
		Assert.That(jobs[0].Title, Is.EqualTo("Software Developer"));
		Assert.That(
			await reader.Credentials.GetAllActiveAsync().ConfigureAwait(false),
			Is.Empty);
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
		Assert.That(manager.Jobs, Is.InstanceOf<IJobRepository>());
	}

	[Test]
	public void Dispose_PreventsFurtherServiceAccess()
	{
		using LibraryManager manager = LibraryManager.Initialize(TestDbPath);
		IJobRepository repository = manager.Jobs;
		manager.Dispose();
		manager.Dispose();

		Assert.Throws<ObjectDisposedException>(() => _ = manager.Jobs);
		Assert.Throws<ObjectDisposedException>(() => _ = manager.JobQueries);
		Assert.Throws<ObjectDisposedException>(() => _ = manager.Credentials);
		Assert.Throws<ObjectDisposedException>(() => _ = manager.Refresh);
		Assert.ThrowsAsync<ObjectDisposedException>(
			async () => await repository.GetAllAsync().ConfigureAwait(false));
	}
}
