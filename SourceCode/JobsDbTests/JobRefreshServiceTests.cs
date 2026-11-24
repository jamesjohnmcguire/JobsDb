/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRefreshServiceTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

/// <summary>
/// Covers database ownership, overlap, and retry during acquisition.
/// </summary>
[TestFixture]
internal sealed class JobRefreshServiceTests : BaseTestsSupport
{
	private readonly List<JobsDbContext> contexts = new List<JobsDbContext>();

	/// <summary>
	/// Prepares the refresh database and resets tracked contexts.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		contexts.Clear();
		DatabaseTestSchema.Create(TestDbPath!);
	}

	/// <summary>
	/// Disposes any remaining contexts and removes the test database.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		foreach (JobsDbContext context in contexts)
		{
			context.Dispose();
		}

		using JobsDbContext cleanup = new JobsDbContext(TestDbPath!);
		cleanup.Database.EnsureDeleted();
	}

	/// <summary>
	/// Checks that refresh persists writes and disposes its context.
	/// </summary>
	/// <param name="allSources">
	/// Whether to refresh all sources or one source.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false)]
	[TestCase(true)]
	public async Task Refresh_CompletesWritesAndDisposesSession(bool allSources)
	{
		JobRefreshService service = new JobRefreshService(
			CreateContext, context =>
			{
				Mock<IJobRefreshService> acquisition =
					new Mock<IJobRefreshService>();
				acquisition.Setup(instance => instance.RefreshJobsAsync())
					.Returns(async () => new List<ScraperResult>
					{
						await SaveJobAsync(context).ConfigureAwait(false)
					});
				acquisition.Setup(
					instance => instance.RefreshSourceAsync("Test"))
					.Returns(() => SaveJobAsync(context));
				return acquisition.Object;
			});

		await RunRefreshAsync(service, allSources).ConfigureAwait(false);

		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Single().Title, Is.EqualTo(TestJob!.Title));
		Assert.That(contexts, Has.Count.EqualTo(1));
		AssertDisposed(contexts[0]);
	}

	/// <summary>
	/// Checks that overlapping refreshes are rejected.
	/// </summary>
	/// <param name="firstAllSources">
	/// Whether the first refresh requests all sources.
	/// </param>
	/// <param name="secondAllSources">
	/// Whether the overlapping refresh requests all sources.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false, false)]
	[TestCase(false, true)]
	[TestCase(true, false)]
	[TestCase(true, true)]
	public async Task Refresh_RejectsOverlappingSessions(
		bool firstAllSources, bool secondAllSources)
	{
		TaskCompletionSource<ScraperResult> pending =
			new TaskCompletionSource<ScraperResult>(
				TaskCreationOptions.RunContinuationsAsynchronously);
		Mock<IJobRefreshService> acquisition = new Mock<IJobRefreshService>();
		acquisition.Setup(instance => instance.RefreshJobsAsync())
			.Returns(async () => new List<ScraperResult>
			{
				await pending.Task.ConfigureAwait(false)
			});
		acquisition.Setup(instance => instance.RefreshSourceAsync("Test"))
			.Returns(pending.Task);
		JobRefreshService service = new JobRefreshService(
			CreateContext, context => acquisition.Object);

		Task firstRefresh = RunRefreshAsync(service, firstAllSources);
		try
		{
			Assert.That(firstRefresh.IsCompleted, Is.False);
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => RunRefreshAsync(service, secondAllSources)
					.WaitAsync(TimeSpan.FromSeconds(10)))
				.ConfigureAwait(false);
			Assert.That(contexts, Has.Count.EqualTo(1));
			Assert.That(contexts[0].Jobs.Count(), Is.Zero);
		}
		finally
		{
			pending.SetResult(new ScraperResult { Success = true });
			await firstRefresh.ConfigureAwait(false);
		}

		AssertDisposed(contexts[0]);
		await RunRefreshAsync(service, secondAllSources).ConfigureAwait(false);
		Assert.That(contexts, Has.Count.EqualTo(2));
		AssertDisposed(contexts[1]);
	}

	/// <summary>
	/// Checks that a failed refresh disposes its session and permits retry.
	/// </summary>
	/// <param name="failDuringRegistration">
	/// Whether to fail while constructing the acquisition service.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false)]
	[TestCase(true)]
	public async Task Refresh_FailureDisposesSessionAndAllowsRetry(
		bool failDuringRegistration)
	{
		int attempt = 0;
		JobRefreshService service = new JobRefreshService(
			CreateContext, context =>
			{
				attempt++;
				if (attempt == 1 && failDuringRegistration)
				{
					throw new InvalidOperationException("Registration failed");
				}

				Mock<IJobRefreshService> acquisition =
					new Mock<IJobRefreshService>();
				if (attempt == 1)
				{
					acquisition.Setup(instance => instance.RefreshJobsAsync())
						.ThrowsAsync(
							new InvalidOperationException("Run failed"));
				}
				else
				{
					acquisition.Setup(instance => instance.RefreshJobsAsync())
						.ReturnsAsync(new List<ScraperResult>());
				}

				return acquisition.Object;
			});

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => service.RefreshJobsAsync()).ConfigureAwait(false);
		AssertDisposed(contexts[0]);

		await service.RefreshJobsAsync().ConfigureAwait(false);

		Assert.That(contexts, Has.Count.EqualTo(2));
		Assert.That(contexts[0], Is.Not.SameAs(contexts[1]));
		AssertDisposed(contexts[1]);
	}

	/// <summary>
	/// Checks that a context creation failure does not prevent a later refresh.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Refresh_ContextCreationFailureAllowsRetry()
	{
		bool failCreation = true;
		Mock<IJobRefreshService> acquisition = new Mock<IJobRefreshService>();
		acquisition.Setup(instance => instance.RefreshJobsAsync())
			.ReturnsAsync(new List<ScraperResult>());
		JobRefreshService service = new JobRefreshService(
			() =>
			{
				if (failCreation)
				{
					throw new InvalidOperationException(
						"Context creation failed");
				}

				return CreateContext();
			},
			context => acquisition.Object);

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => service.RefreshJobsAsync()).ConfigureAwait(false);
		failCreation = false;

		await service.RefreshJobsAsync().ConfigureAwait(false);

		Assert.That(contexts, Has.Count.EqualTo(1));
		AssertDisposed(contexts[0]);
	}

	private static void AssertDisposed(JobsDbContext context)
	{
		Assert.Throws<ObjectDisposedException>(() => context.Jobs.Count());
	}

	private static async Task RunRefreshAsync(
		IJobRefreshService service, bool allSources)
	{
		if (allSources)
		{
			await service.RefreshJobsAsync().ConfigureAwait(false);
		}
		else
		{
			await service.RefreshSourceAsync("Test").ConfigureAwait(false);
		}
	}

	private JobsDbContext CreateContext()
	{
		JobsDbContext context = new JobsDbContext(TestDbPath!);
		contexts.Add(context);
		return context;
	}

	private async Task<ScraperResult> SaveJobAsync(JobsDbContext context)
	{
		Job job = CopyJob(TestJob!);
		context.Jobs.Add(job);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return new ScraperResult { Success = true, JobsAdded = 1 };
	}
}
