/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobRepositoryTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Repositories;

using NUnit.Framework;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobsDbLibrary.Scrapers;

[TestFixture]
internal sealed class JobRepositoryTests : BaseTestsSupport
{
    private JobsDbContext _context;
    private JobRepository _repository;
    private string _testDbPath;

    [SetUp]
    public void SetUp()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_jobs_{Guid.NewGuid()}.db");
        _context = new JobsDbContext(_testDbPath);
        _context.Database.EnsureCreated();
        _repository = new JobRepository(_context);
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
	public async Task AddAsync_ValidJob_AddsToDatabase()
	{
		var result = await _repository.AddAsync(TestJob);

		Assert.That(result.Id, Is.GreaterThan(0));
		Assert.That(result.DateScraped, Is.Not.EqualTo(default(DateTime)));

		var jobs = await _repository.GetAllAsync();
		Assert.That(jobs.Count, Is.EqualTo(1));
	}

    [Test]
    public async Task GetByIdAsync_ExistingJob_ReturnsJob()
    {
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob);

        // Act
        var result = await _repository.GetByIdAsync(job.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(job.Id));
        Assert.That(result.Title, Is.EqualTo(job.Title));
    }

    [Test]
    public async Task GetByIdAsync_NonExistingJob_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetBySourceIdAsync_ExistingJob_ReturnsJob()
    {
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob);

        // Act
        var result = await _repository.GetBySourceIdAsync(job.Source, job.SourceJobId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SourceJobId, Is.EqualTo(job.SourceJobId));
    }

    [Test]
    public async Task UpdateAsync_ExistingJob_UpdatesDatabase()
    {
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob);

        job.Title = "Updated Title";
        job.Status = ApplicationStatus.Applied;

        // Act
        var result = await _repository.UpdateAsync(job);

        // Assert
        var updated = await _repository.GetByIdAsync(job.Id);
        Assert.That(updated.Title, Is.EqualTo("Updated Title"));
        Assert.That(updated.Status, Is.EqualTo(ApplicationStatus.Applied));
    }

	[Test]
	public async Task DeleteAsync_ExistingJob_RemovesFromDatabase()
	{
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob);

		var result = await _repository.DeleteAsync(job.Id);

		Assert.That(result, Is.True);
		var deleted = await _repository.GetByIdAsync(job.Id);
		Assert.That(deleted, Is.Null);
	}

    [Test]
    public async Task DeleteAsync_NonExistingJob_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetByStatusAsync_FiltersByStatus_ReturnsMatchingJobs()
    {
		Job tempJob = CopyJob(TestJob);
		tempJob.Status = ApplicationStatus.NotApplied;
		Job job = await _repository.AddAsync(tempJob);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Source = "AnotherSource";
		tempJob2.Status = ApplicationStatus.Applied;
		Job job2 = await _repository.AddAsync(tempJob2);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Source = "AnotherSourceAgain";
		tempJob3.Status = ApplicationStatus.Applied;
		Job job3 = await _repository.AddAsync(tempJob3);

        // Act
        var appliedJobs = await _repository.GetByStatusAsync(ApplicationStatus.Applied);

        // Assert
        Assert.That(appliedJobs.Count, Is.EqualTo(2));
        Assert.That(appliedJobs.All(j => j.Status == ApplicationStatus.Applied), Is.True);
    }

    [Test]
    public async Task GetActiveJobsAsync_ExcludesArchived_ReturnsOnlyActive()
    {
		Job tempJob = CopyJob(TestJob);
		tempJob.IsArchived = false;
		Job job = await _repository.AddAsync(tempJob);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.IsArchived = false;
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.IsArchived = true;
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3);

        // Act
        var activeJobs = await _repository.GetActiveJobsAsync();

        // Assert
        Assert.That(activeJobs.Count, Is.EqualTo(2));
        Assert.That(activeJobs.All(j => !j.IsArchived), Is.True);
    }

    [Test]
    public async Task SearchAsync_ByTitle_ReturnsMatchingJobs()
    {
		Job tempJob = CopyJob(TestJob);
		tempJob.Title = "Senior Software Engineer";
		Job job = await _repository.AddAsync(tempJob);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Title = "Junior Developer";
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Title = "Senior DevOps Engineer";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3);

        // Act
        var results = await _repository.SearchAsync("Senior");

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.All(j => j.Title.Contains("Senior")), Is.True);
    }

    [Test]
    public async Task SearchAsync_ByCompany_ReturnsMatchingJobs()
    {
		Job tempJob = CopyJob(TestJob);
		tempJob.Company = "Google";
		Job job = await _repository.AddAsync(tempJob);

		Job tempJob2 = CopyJob(TestJob);
		tempJob2.Company = "Microsoft";
		tempJob2.Source = "AnotherSource";
		Job job2 = await _repository.AddAsync(tempJob2);

		Job tempJob3 = CopyJob(TestJob);
		tempJob3.Company = "Amazon";
		tempJob3.Source = "AnotherSourceAgain";
		Job job3 = await _repository.AddAsync(tempJob3);

        // Act
        var results = await _repository.SearchAsync("Microsoft");

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Company, Is.EqualTo("Microsoft"));
    }

    [Test]
    public async Task ExistsAsync_ExistingJob_ReturnsTrue()
    {
		Job tempJob = CopyJob(TestJob);
		Job job = await _repository.AddAsync(tempJob);

        // Act
        var exists = await _repository.ExistsAsync(job.Source, job.SourceJobId);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_NonExistingJob_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync("LinkedIn", "nonexistent");

        // Assert
        Assert.That(exists, Is.False);
    }

    private Job CreateTestJob(
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

[TestFixture]
public class CredentialRepositoryTests
{
    private JobsDbContext _context;
    private CredentialRepository _repository;
    private string _testDbPath;

    [SetUp]
    public void SetUp()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_creds_{Guid.NewGuid()}.db");
        _context = new JobsDbContext(_testDbPath);
        _context.Database.EnsureCreated();
        _repository = new CredentialRepository(_context);
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
    public async Task AddOrUpdateAsync_NewCredential_AddsToDatabase()
    {
        // Arrange
        var credential = new ScraperCredential
        {
			CookieData = "cookie",
			Source = "LinkedIn",
            Username = "test@example.com",
            EncryptedPassword = "encrypted",
            IsActive = true
        };

        // Act
        var result = await _repository.AddOrUpdateAsync(credential);

        // Assert
        Assert.That(result.Id, Is.GreaterThan(0));
        var retrieved = await _repository.GetBySourceAsync("LinkedIn");
        Assert.That(retrieved, Is.Not.Null);
    }

    [Test]
    public async Task AddOrUpdateAsync_ExistingCredential_UpdatesInDatabase()
    {
        // Arrange
        var credential = new ScraperCredential
        {
			CookieData = "cookie",
			Source = "LinkedIn",
            Username = "old@example.com",
            EncryptedPassword = "encrypted",
            IsActive = true
        };
        await _repository.AddOrUpdateAsync(credential);

        credential.Username = "new@example.com";

        // Act
        var result = await _repository.AddOrUpdateAsync(credential);

        // Assert
        var retrieved = await _repository.GetBySourceAsync("LinkedIn");
        Assert.That(retrieved.Username, Is.EqualTo("new@example.com"));
    }

    [Test]
    public async Task GetBySourceAsync_ExistingCredential_ReturnsCredential()
    {
        // Arrange
        var credential = new ScraperCredential
        {
			CookieData = "cookie",
			Source = "TokyoDev",
            Username = "test@example.com",
            EncryptedPassword = "encrypted",
            IsActive = true
        };
        await _repository.AddOrUpdateAsync(credential);

        // Act
        var result = await _repository.GetBySourceAsync("TokyoDev");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Source, Is.EqualTo("TokyoDev"));
    }

    [Test]
    public async Task GetBySourceAsync_NonExistingSource_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBySourceAsync("NonExistent");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllActiveAsync_OnlyReturnsActive()
    {
        // Arrange
        await _repository.AddOrUpdateAsync(new ScraperCredential
        {
			CookieData = "cookie",
			Source = "LinkedIn",
            Username = "test1@example.com",
            EncryptedPassword = "encrypted",
            IsActive = true
        });
        await _repository.AddOrUpdateAsync(new ScraperCredential
        {
			CookieData = "cookie",
			Source = "TokyoDev",
            Username = "test2@example.com",
            EncryptedPassword = "encrypted",
            IsActive = false
        });

        // Act
        var active = await _repository.GetAllActiveAsync();

        // Assert
        Assert.That(active.Count, Is.EqualTo(1));
        Assert.That(active[0].IsActive, Is.True);
    }
}
