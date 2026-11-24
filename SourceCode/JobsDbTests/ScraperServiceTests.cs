/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperServiceTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

/// <summary>
/// Covers scraper registration, execution, and persisted run logs.
/// </summary>
[TestFixture]
internal sealed class ScraperServiceTests
{
	private JobsDbContext context;
	private ScraperService service;
	private string testDbPath;

	/// <summary>
	/// Prepares the database context and scraper service for the test.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_scraper_{Guid.NewGuid()}.db");
		DatabaseTestSchema.Create(testDbPath);
		context = new JobsDbContext(testDbPath);
		service = new ScraperService(context);
	}

	/// <summary>
	/// Disposes the test context and removes the database.
	/// </summary>
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

	/// <summary>
	/// Checks that a registered scraper appears in the source list.
	/// </summary>
	[Test]
	public void RegisterScraper_ValidScraper_IsRegistered()
	{
		// Arrange
		Mock<JobScraperBase> mockScraper = new Mock<JobScraperBase>(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>(),
			"TestSource");

		// Act
		service.RegisterScraper("TestSource", mockScraper.Object);
		var sources = service.GetRegisteredSources();

		// Assert
		Assert.That(sources, Contains.Item("TestSource"));
	}

	/// <summary>
	/// Checks that running an unknown source returns a failed result.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task RunScraperAsync_UnregisteredSource_ReturnsErrorResult()
	{
		// Act
		var result = await service.RunScraperAsync("NonExistentSource").ConfigureAwait(false);

		// Assert
		Assert.That(result.Success, Is.False);
		Assert.That(result.ErrorMessage, Does.Contain("No scraper registered"));
	}

	/// <summary>
	/// Checks that running a registered source executes it and saves a log.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task RunScraperAsync_RegisteredScraper_ExecutesAndLogsResult()
	{
		// Arrange
		MockTestScraper mockScraper = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());

		service.RegisterScraper("TestSource", mockScraper);

		// Act
		var result = await service.RunScraperAsync("TestSource").ConfigureAwait(false);

		// Assert
		Assert.That(result.Success, Is.True);
		Assert.That(result.JobsFound, Is.EqualTo(5));

		// Verify log was created
		List<ScraperLog> logs = context.ScraperLogs.ToList();
		Assert.That(logs.Count, Is.EqualTo(1));
		Assert.That(logs[0].Source, Is.EqualTo("TestSource"));
		Assert.That(logs[0].Success, Is.True);
	}

	/// <summary>
	/// Checks that a refresh runs all registered scrapers.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task RunAllScrapersAsync_MultipleScrapers_RunsAll()
	{
		// Arrange
		MockTestScraper scraper1 = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());
		MockTestScraper scraper2 = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());

		service.RegisterScraper("Source1", scraper1);
		service.RegisterScraper("Source2", scraper2);

		// Act
		var results = await service.RunAllScrapersAsync().ConfigureAwait(false);

		// Assert
		Assert.That(results.Count, Is.EqualTo(2));
		Assert.That(results.All(r => r.Success), Is.True);
	}

	/// <summary>
	/// Checks that source enumeration includes each registered scraper.
	/// </summary>
	[Test]
	public void GetRegisteredSources_MultipleScrapers_ReturnsAllSources()
	{
		// Arrange
		MockTestScraper scraper1 = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());
		MockTestScraper scraper2 = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());

		service.RegisterScraper("LinkedIn", scraper1);
		service.RegisterScraper("TokyoDev", scraper2);

		// Act
		var sources = service.GetRegisteredSources();

		// Assert
		Assert.That(sources.Count, Is.EqualTo(2));
		Assert.That(sources, Contains.Item("LinkedIn"));
		Assert.That(sources, Contains.Item("TokyoDev"));
	}

	/// <summary>
	/// Checks that refresh waits until records have been persisted.
	/// </summary>
	/// <param name="refreshAll">
	/// Whether the refresh runs all sources or one source.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false)]
	[TestCase(true)]
	public async Task Refresh_WaitsForPersistedResults(bool refreshAll)
	{
		MockTestScraper scraper = new MockTestScraper(
			Mock.Of<IJobRepository>(),
			Mock.Of<ICredentialRepository>());
		service.RegisterScraper("TestSource", scraper);
		IJobRefreshService refreshService = service;
		ScraperResult result;

		if (refreshAll)
		{
			List<ScraperResult> results =
				await refreshService.RefreshJobsAsync().ConfigureAwait(false);
			Assert.That(results, Has.Count.EqualTo(1));
			result = results[0];
		}
		else
		{
			result = await refreshService.RefreshSourceAsync("TestSource")
				.ConfigureAwait(false);
		}

		Assert.That(result.Success, Is.True);
		Assert.That(result.JobsAdded, Is.EqualTo(3));
		using JobsDbContext reader = new JobsDbContext(testDbPath);
		ScraperLog log = reader.ScraperLogs.Single();
		Assert.That(log.Source, Is.EqualTo("TestSource"));
		Assert.That(log.JobsAdded, Is.EqualTo(result.JobsAdded));
		Assert.That(log.Success, Is.True);
	}

	// Mock scraper for testing
	private sealed class MockTestScraper : JobScraperBase
	{
		public MockTestScraper(
			IJobRepository jobRepository,
			ICredentialRepository credentialRepository)
			: base(jobRepository, credentialRepository, "TestSource")
		{
		}

		public override Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
		{
			return Task.FromResult(new ScraperResult
			{
				Success = true,
				JobsFound = 5,
				JobsAdded = 3,
				JobsUpdated = 2,
				Duration = TimeSpan.FromSeconds(10)
			});
		}

		protected override Task<bool> LoginAsync(ScraperCredential credential)
		{
			return Task.FromResult(true);
		}
	}
}
