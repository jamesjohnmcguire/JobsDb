/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialEncryptionTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Services;

using NUnit.Framework;
using JobsDb.Core.Services;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDb.Core.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JobsDb.Core;
using JobsDbLibrary.Scrapers;
using JobsDb.Core.Scrapers;
using System.Linq;

[TestFixture]
public class CredentialEncryptionTests
{
    private const string MasterPassword = "TestPassword123!";

    [Test]
    public void Encrypt_ValidText_ReturnsEncryptedString()
    {
        // Arrange
        var plainText = "MySecretPassword";

        // Act
        var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

        // Assert
        Assert.That(encrypted, Is.Not.Null);
        Assert.That(encrypted, Is.Not.Empty);
        Assert.That(encrypted, Is.Not.EqualTo(plainText));
    }

    [Test]
    public void Decrypt_EncryptedText_ReturnsOriginalString()
    {
        // Arrange
        var plainText = "MySecretPassword";
        var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

        // Act
        var decrypted = CredentialEncryption.Decrypt(encrypted, MasterPassword);

        // Assert
        Assert.That(decrypted, Is.EqualTo(plainText));
    }

    [Test]
    public void Encrypt_SameTextTwice_ProducesSameResult()
    {
        // Arrange
        var plainText = "MySecretPassword";

        // Act
        var encrypted1 = CredentialEncryption.Encrypt(plainText, MasterPassword);
        var encrypted2 = CredentialEncryption.Encrypt(plainText, MasterPassword);

        // Assert
        Assert.That(encrypted1, Is.EqualTo(encrypted2));
    }

    [Test]
    public void Decrypt_WrongPassword_ThrowsException()
    {
        // Arrange
        var plainText = "MySecretPassword";
        var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

        // Act & Assert
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
        {
            CredentialEncryption.Decrypt(encrypted, "WrongPassword");
        });
    }

    [Test]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        // Act
        var result = CredentialEncryption.Encrypt("", MasterPassword);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Decrypt_EmptyString_ReturnsEmptyString()
    {
        // Act
        var result = CredentialEncryption.Decrypt("", MasterPassword);

        // Assert
        Assert.That(result, Is.Empty);
    }
}

[TestFixture]
public class ScraperResultTests
{
    [Test]
    public void ScraperResult_NewInstance_HasDefaultValues()
    {
        // Act
        var result = new ScraperResult();

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.JobsFound, Is.EqualTo(0));
        Assert.That(result.JobsAdded, Is.EqualTo(0));
        Assert.That(result.JobsUpdated, Is.EqualTo(0));
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Jobs, Is.Not.Null);
        Assert.That(result.Jobs.Count, Is.EqualTo(0));
    }

    [Test]
    public void ScraperResult_SetProperties_ValuesAreSet()
    {
        // Arrange
        var result = new ScraperResult();
        var duration = TimeSpan.FromSeconds(30);

        // Act
        result.Success = true;
        result.JobsFound = 10;
        result.JobsAdded = 5;
        result.JobsUpdated = 3;
        result.Duration = duration;
        result.Jobs.Add(new Job { Title = "Test" });

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.JobsFound, Is.EqualTo(10));
        Assert.That(result.JobsAdded, Is.EqualTo(5));
        Assert.That(result.JobsUpdated, Is.EqualTo(3));
        Assert.That(result.Duration, Is.EqualTo(duration));
        Assert.That(result.Jobs.Count, Is.EqualTo(1));
    }
}

[TestFixture]
public class ScraperServiceTests
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
        var scraper1 = new MockTestScraper(
            Mock.Of<IJobRepository>(),
            Mock.Of<ICredentialRepository>());
        var scraper2 = new MockTestScraper(
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

[TestFixture]
public class CookieManagerTests
{
    private CookieManager _cookieManager;
    private string _testCookiesPath;

    [SetUp]
    public void SetUp()
    {
        _cookieManager = new CookieManager();
        _testCookiesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "JobsDb",
            "Cookies");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test cookies
        if (Directory.Exists(_testCookiesPath))
        {
            var testFiles = Directory.GetFiles(_testCookiesPath, "test_*.json");
            foreach (var file in testFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    [Test]
    public void ClearCookies_NonExistentSource_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _cookieManager.ClearCookies("NonExistent"));
    }

    [Test]
    public void HasValidCookies_NonExistentSource_ReturnsFalse()
    {
        // Act
        var result = _cookieManager.HasValidCookies("NonExistent");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ClearCookies_ExistingCookies_RemovesFile()
    {
        // Arrange
        var testSource = "test_source_" + Guid.NewGuid();
        var cookiePath = Path.Combine(_testCookiesPath, $"{testSource.ToLower()}_cookies.json");
        Directory.CreateDirectory(_testCookiesPath);
        File.WriteAllText(cookiePath, "[]");

        // Act
        _cookieManager.ClearCookies(testSource);

        // Assert
        Assert.That(File.Exists(cookiePath), Is.False);
    }
}
