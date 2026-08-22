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
using DigitalZenWorks.JobsDb.Library.Data;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

[TestFixture]
internal class ScraperServiceTests
{
	private JobsDbContext context;
	private ScraperService service;
	private string testDbPath;

	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_scraper_{Guid.NewGuid()}.db");
		context = new JobsDbContext(testDbPath);
		context.Database.EnsureCreated();
		service = new ScraperService(context);
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

	[Test]
	public async Task RunScraperAsync_UnregisteredSource_ReturnsErrorResult()
	{
		// Act
		var result = await service.RunScraperAsync("NonExistentSource").ConfigureAwait(false);

		// Assert
		Assert.That(result.Success, Is.False);
		Assert.That(result.ErrorMessage, Does.Contain("No scraper registered"));
	}

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

	// Mock scraper for testing
	private class MockTestScraper : JobScraperBase
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
