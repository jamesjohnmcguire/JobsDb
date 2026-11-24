/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseJobReaderTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;

[TestFixture]
internal sealed class DatabaseJobReaderTests : BaseTestsSupport
{
	[SetUp]
	public void SetUp()
	{
		DatabaseTestSchema.Create(TestDbPath!);
	}

	[TearDown]
	public void TearDown()
	{
		using JobsDbContext cleanup = new JobsDbContext(TestDbPath!);
		cleanup.Database.EnsureDeleted();
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task Read_EachCallOwnsAndDisposesItsContext(bool readAll)
	{
		List<JobsDbContext> contexts = new List<JobsDbContext>();
		DatabaseJobReader reader = new DatabaseJobReader(() =>
		{
			JobsDbContext context = new JobsDbContext(TestDbPath!);
			contexts.Add(context);
			return context;
		});

		for (int iteration = 0; iteration < 2; iteration++)
		{
			List<Job> jobs;
			if (readAll)
			{
				jobs = await reader.GetAllAsync().ConfigureAwait(false);
			}
			else
			{
				jobs = await reader.GetActiveJobsAsync().ConfigureAwait(false);
			}

			Assert.That(jobs, Is.Empty);
		}

		Assert.That(contexts, Has.Count.EqualTo(2));
		Assert.That(contexts[0], Is.Not.SameAs(contexts[1]));
		foreach (JobsDbContext context in contexts)
		{
			Assert.Throws<ObjectDisposedException>(
				() => context.Jobs.Count());
		}
	}

	[Test]
	public async Task Read_FailureStillDisposesContext()
	{
		using (JobsDbContext preparation = new JobsDbContext(TestDbPath!))
		{
			preparation.Database.ExecuteSqlRaw("DROP TABLE Jobs");
		}

		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		DatabaseJobReader reader = new DatabaseJobReader(() => context);

		await Assert.ThrowsAsync<SqliteException>(
			() => reader.GetAllAsync()).ConfigureAwait(false);
		Assert.Throws<ObjectDisposedException>(() => context.Jobs.Count());
	}

	[Test]
	public async Task Read_OverlappingQueriesUseIndependentContexts()
	{
		PausedReadInterceptor interceptor = new PausedReadInterceptor();
		using JobsDbContext template = new JobsDbContext(TestDbPath!);
		DbContextOptionsBuilder<JobsDbContext> options =
			new DbContextOptionsBuilder<JobsDbContext>();
		options.UseSqlite(template.Database.GetConnectionString());
		options.AddInterceptors(interceptor);
		DatabaseJobReader reader = new DatabaseJobReader(
			() => new JobsDbContext(options.Options));

		Task<List<Job>> firstRead = reader.GetAllAsync();
		try
		{
			await interceptor.FirstReadStarted
				.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
			List<Job> jobs = await reader.GetActiveJobsAsync()
				.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

			Assert.That(jobs, Is.Empty);
			Assert.That(firstRead.IsCompleted, Is.False);
		}
		finally
		{
			interceptor.ResumeFirstRead();
			await firstRead.WaitAsync(TimeSpan.FromSeconds(10))
				.ConfigureAwait(false);
		}
	}

	private sealed class PausedReadInterceptor : DbCommandInterceptor
	{
		private readonly TaskCompletionSource firstReadStarted =
			new TaskCompletionSource(
				TaskCreationOptions.RunContinuationsAsynchronously);

		private readonly TaskCompletionSource resumeFirstRead =
			new TaskCompletionSource(
				TaskCreationOptions.RunContinuationsAsynchronously);

		private int commandCount;

		public Task FirstReadStarted
		{
			get
			{
				return firstReadStarted.Task;
			}
		}

		public void ResumeFirstRead()
		{
			resumeFirstRead.SetResult();
		}

		public override async ValueTask<InterceptionResult<DbDataReader>>
			ReaderExecutingAsync(
				DbCommand command,
				CommandEventData eventData,
				InterceptionResult<DbDataReader> result,
				CancellationToken cancellationToken = default)
		{
			if (Interlocked.Increment(ref commandCount) == 1)
			{
				firstReadStarted.SetResult();
				await resumeFirstRead.Task.WaitAsync(cancellationToken)
					.ConfigureAwait(false);
			}

			return result;
		}
	}
}
