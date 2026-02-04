namespace JobsDbTests
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using JobsDb.Core.Data;
	using JobsDb.Core.Repositories;
	using JobsDb.Core.Scrapers;
	using JobsDbLibrary.Scrapers;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	internal class ScraperServiceTests
	{
		private JobsDbContext _context;
		private ScraperService _service;
		private string _testDbPath;

		[SetUp]
		public void SetUp()
		{
			_testDbPath = Path.Combine(Path.GetTempPath(), $"test_scraper_{Guid.NewGuid()}.db");
			_context = new JobsDbContext(_testDbPath);
			_context.Database.EnsureCreated();
			_service = new ScraperService(_context);
		}

		[TearDown]
		public void TearDown()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();

			if (File.Exists(_testDbPath))
				File.Delete(_testDbPath);
		}

		[Test]
		public void RegisterScraper_ValidScraper_IsRegistered()
		{
			// Arrange
			var mockScraper = new Mock<JobScraperBase>(
				Mock.Of<IJobRepository>(),
				Mock.Of<ICredentialRepository>(),
				"TestSource");

			// Act
			_service.RegisterScraper("TestSource", mockScraper.Object);
			var sources = _service.GetRegisteredSources();

			// Assert
			Assert.That(sources, Contains.Item("TestSource"));
		}

		[Test]
		public async Task RunScraperAsync_UnregisteredSource_ReturnsErrorResult()
		{
			// Act
			var result = await _service.RunScraperAsync("NonExistentSource");

			// Assert
			Assert.That(result.Success, Is.False);
			Assert.That(result.ErrorMessage, Does.Contain("No scraper registered"));
		}

		[Test]
		public async Task RunScraperAsync_RegisteredScraper_ExecutesAndLogsResult()
		{
			// Arrange
			var mockScraper = new MockTestScraper(
				Mock.Of<IJobRepository>(),
				Mock.Of<ICredentialRepository>());

			_service.RegisterScraper("TestSource", mockScraper);

			// Act
			var result = await _service.RunScraperAsync("TestSource");

			// Assert
			Assert.That(result.Success, Is.True);
			Assert.That(result.JobsFound, Is.EqualTo(5));

			// Verify log was created
			var logs = _context.ScraperLogs.ToList();
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

			_service.RegisterScraper("Source1", scraper1);
			_service.RegisterScraper("Source2", scraper2);

			// Act
			var results = await _service.RunAllScrapersAsync();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2));
			Assert.That(results.All(r => r.Success), Is.True);
		}

		[Test]
		public void GetRegisteredSources_MultipleScrapers_ReturnsAllSources()
		{
			// Arrange
			var scraper1 = new MockTestScraper(
				Mock.Of<IJobRepository>(),
				Mock.Of<ICredentialRepository>());
			var scraper2 = new MockTestScraper(
				Mock.Of<IJobRepository>(),
				Mock.Of<ICredentialRepository>());

			_service.RegisterScraper("LinkedIn", scraper1);
			_service.RegisterScraper("TokyoDev", scraper2);

			// Act
			var sources = _service.GetRegisteredSources();

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
}
