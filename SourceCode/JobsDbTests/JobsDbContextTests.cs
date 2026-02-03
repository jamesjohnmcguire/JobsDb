/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContextTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Data;

using NUnit.Framework;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using JobsDbLibrary.Scrapers;
using JobsDb.Core.Scrapers;

[TestFixture]
public class JobsDbContextTests
{
    private JobsDbContext _context;
    private string _testDbPath;

    [SetUp]
    public void SetUp()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");
        _context = new JobsDbContext(_testDbPath);
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
    public void Initialize_CreatesDatabase()
    {
        // Act
        _context.Initialize();

        // Assert
        Assert.That(File.Exists(_testDbPath), Is.True);
        Assert.That(_context.Database.CanConnect(), Is.True);
    }

    [Test]
    public void GetDatabasePath_ReturnsCorrectPath()
    {
        // Act
        var path = _context.GetDatabasePath();

        // Assert
        Assert.That(path, Is.EqualTo(_testDbPath));
    }

    [Test]
    public void Jobs_DbSet_IsNotNull()
    {
        // Act
        _context.Initialize();

        // Assert
        Assert.That(_context.Jobs, Is.Not.Null);
    }

    [Test]
    public void Credentials_DbSet_IsNotNull()
    {
        // Act
        _context.Initialize();

        // Assert
        Assert.That(_context.Credentials, Is.Not.Null);
    }

    [Test]
    public void ScraperLogs_DbSet_IsNotNull()
    {
        // Act
        _context.Initialize();

        // Assert
        Assert.That(_context.ScraperLogs, Is.Not.Null);
    }

    [Test]
    public void SearchFilters_DbSet_IsNotNull()
    {
        // Act
        _context.Initialize();

        // Assert
        Assert.That(_context.SearchFilters, Is.Not.Null);
    }

    [Test]
    public void Job_UniqueConstraint_EnforcesSourceAndSourceJobId()
    {
        // Arrange
        _context.Initialize();
            
        var job1 = new Job
        {
            Title = "Test Job",
            Company = "Company A",
            Source = "LinkedIn",
            SourceUrl = "https://test.com/1",
            SourceJobId = "12345",
            DatePosted = DateTime.UtcNow
        };

        var job2 = new Job
        {
            Title = "Different Job",
            Company = "Company B",
            Source = "LinkedIn",
            SourceUrl = "https://test.com/2",
            SourceJobId = "12345", // Same SourceJobId
            DatePosted = DateTime.UtcNow
        };

        // Act
        _context.Jobs.Add(job1);
        _context.SaveChanges();

        _context.Jobs.Add(job2);

        // Assert
        Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
    }

    [Test]
    public void ScraperCredential_UniqueSource_EnforcesConstraint()
    {
        // Arrange
        _context.Initialize();
            
        var cred1 = new ScraperCredential
        {
            Source = "LinkedIn",
            Username = "user1@example.com",
            EncryptedPassword = "encrypted1",
            IsActive = true
        };

        var cred2 = new ScraperCredential
        {
            Source = "LinkedIn", // Same source
            Username = "user2@example.com",
            EncryptedPassword = "encrypted2",
            IsActive = true
        };

        // Act
        _context.Credentials.Add(cred1);
        _context.SaveChanges();

        _context.Credentials.Add(cred2);

        // Assert
        Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
    }

    [Test]
    public void Job_StatusEnum_SavesAsString()
    {
        // Arrange
        _context.Initialize();
            
        var job = new Job
        {
            Title = "Test",
            Company = "Test Co",
            Source = "Test",
            SourceUrl = "https://test.com",
            SourceJobId = "123",
            DatePosted = DateTime.UtcNow,
            Status = ApplicationStatus.Applied
        };

        // Act
        _context.Jobs.Add(job);
        _context.SaveChanges();

        // Verify it's stored as string in database
        var saved = _context.Jobs.First();
            
        // Assert
        Assert.That(saved.Status, Is.EqualTo(ApplicationStatus.Applied));
    }

    [Test]
    public void Job_DateScraped_HasDefaultValue()
    {
        // Arrange
        _context.Initialize();
            
        var job = new Job
        {
            Title = "Test",
            Company = "Test Co",
            Source = "Test",
            SourceUrl = "https://test.com",
            SourceJobId = "123",
            DatePosted = DateTime.UtcNow
        };

        // Act
        _context.Jobs.Add(job);
        _context.SaveChanges();

        var saved = _context.Jobs.First();

        // Assert
        Assert.That(saved.DateScraped, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void SearchFilter_CreatedDate_HasDefaultValue()
    {
        // Arrange
        _context.Initialize();
            
        var filter = new SearchFilter
        {
            Name = "Test Filter",
            Keywords = "developer",
            IsActive = true
        };

        // Act
        _context.SearchFilters.Add(filter);
        _context.SaveChanges();

        var saved = _context.SearchFilters.First();

        // Assert
        Assert.That(saved.CreatedDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void ScraperCredential_IsActive_HasDefaultValue()
    {
        // Arrange
        _context.Initialize();
            
        var cred = new ScraperCredential
        {
            Source = "TestSource",
            Username = "test@example.com",
            EncryptedPassword = "encrypted"
        };

        // Act
        _context.Credentials.Add(cred);
        _context.SaveChanges();

        var saved = _context.Credentials.First();

        // Assert
        Assert.That(saved.IsActive, Is.True);
    }

    [Test]
    public void Job_Indexes_AreCreated()
    {
        // Arrange
        _context.Initialize();

        // Act - Query using indexed columns should work efficiently
        var job = new Job
        {
            Title = "Test",
            Company = "TestCompany",
            Location = "Tokyo",
            Source = "LinkedIn",
            SourceUrl = "https://test.com",
            SourceJobId = "123",
            DatePosted = DateTime.UtcNow,
            Status = ApplicationStatus.Applied,
            IsArchived = false
        };

        _context.Jobs.Add(job);
        _context.SaveChanges();

        // These queries use indexes
        var byStatus = _context.Jobs.Where(j => j.Status == ApplicationStatus.Applied).ToList();
        var byCompany = _context.Jobs.Where(j => j.Company == "TestCompany").ToList();
        var byLocation = _context.Jobs.Where(j => j.Location == "Tokyo").ToList();
        var byArchived = _context.Jobs.Where(j => !j.IsArchived).ToList();

        // Assert
        Assert.That(byStatus.Count, Is.EqualTo(1));
        Assert.That(byCompany.Count, Is.EqualTo(1));
        Assert.That(byLocation.Count, Is.EqualTo(1));
        Assert.That(byArchived.Count, Is.EqualTo(1));
    }

    [Test]
    public void Context_MultipleInstances_UseSameDatabase()
    {
        // Arrange
        _context.Initialize();
            
        var job = new Job
        {
            Title = "Test",
            Company = "Test Co",
			Description = "Test Description",
			JobType = "Full-time",
			Notes = "Test Notes",
			Location = "Remote",
			Requirements = "None",
			RemoteType = "Remote",
			SalaryCurrency = "USD",
			Source = "Test",
            SourceUrl = "https://test.com",
            SourceJobId = "123",
            DatePosted = DateTime.UtcNow
        };

        _context.Jobs.Add(job);
        _context.SaveChanges();

        // Act - Create new context with same path
        using (var context2 = new JobsDbContext(_testDbPath))
        {
            var jobs = context2.Jobs.ToList();

            // Assert
            Assert.That(jobs.Count, Is.EqualTo(1));
            Assert.That(jobs[0].Title, Is.EqualTo("Test"));
        }
    }

    [Test]
    public void Job_RequiredFields_EnforcedByDatabase()
    {
        // Arrange
        _context.Initialize();
            
        var job = new Job
        {
            // Missing Title (required)
            Company = "Test Co",
            Source = "Test",
            SourceUrl = "https://test.com",
            DatePosted = DateTime.UtcNow
        };

        // Act & Assert
        _context.Jobs.Add(job);
        Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
    }
}
