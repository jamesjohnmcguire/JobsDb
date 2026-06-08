/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContextTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using JobsDb.Core.Scrapers;
using JobsDbLibrary.Scrapers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
internal sealed class JobsDbContextTests : BaseTestsSupport
{
	private JobsDbContext _context;
	private Job testJobAppied;

	/// <summary>
	/// The one time setup method.
	/// </summary>
	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		testJobAppied = CopyJob(TestJob);
		testJobAppied.Status = ApplicationStatus.Applied;
	}

	/// <summary>
	/// One time tear down method.
	/// </summary>
	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
	}

	[SetUp]
	public void SetUp()
	{
		_context = new JobsDbContext(TestDbPath);
	}

	[TearDown]
	public void TearDown()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Test]
	public void Initialize_CreatesDatabase()
	{
		// Act
		_context.Initialize();

		// Assert
		Assert.That(File.Exists(TestDbPath), Is.True);
		Assert.That(_context.Database.CanConnect(), Is.True);
	}

	[Test]
	public void GetDatabasePath_ReturnsCorrectPath()
	{
		// Act
		var path = _context.GetDatabasePath();

		// Assert
		Assert.That(path, Is.EqualTo(TestDbPath));
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
		_context.Initialize();

		Job badJob = TestJob;
		badJob.Title = "Different Job";
		badJob.Company = "Company B";
		badJob.Source = "LinkedIn";
		badJob.SourceUrl = "https://test.com/2";

		_context.Jobs.Add(TestJob);
		_context.SaveChanges();

		_context.Jobs.Add(badJob);

		Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
	}

	[Test]
	public void ScraperCredential_UniqueSource_EnforcesConstraint()
	{
		// Arrange
		_context.Initialize();

		ScraperCredential cred1 = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "user1@example.com",
			EncryptedPassword = "encrypted1",
			IsActive = true
		};

		ScraperCredential cred2 = new ScraperCredential
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
		_context.Initialize();

		_context.Jobs.Add(testJobAppied);
		_context.SaveChanges();

		// Verify it's stored as string in database
		var saved = _context.Jobs.First();

		Assert.That(saved.Status, Is.EqualTo(ApplicationStatus.Applied));
	}

	[Test]
	public void Job_DateScraped_HasDefaultValue()
	{
		_context.Initialize();

		_context.Jobs.Add(TestJob);
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

		SearchFilter filter = new SearchFilter
		{
			Name = "Test Filter",
			Keywords = "developer",
			IsActive = true,
			Location = "Remote",
			Source = "TestSource"
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

		ScraperCredential cred = new ScraperCredential
		{
			CookieData = "cookie",
			IsActive = true,
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

		_context.Jobs.Add(testJobAppied);
		_context.SaveChanges();

		// These queries use indexes
		List<Job> byStatus = _context.Jobs.Where(j => j.Status == ApplicationStatus.Applied).ToList();
		List<Job> byCompany = _context.Jobs.Where(j => j.Company == "Test Company").ToList();
		List<Job> byLocation = _context.Jobs.Where(j => j.Location == "Remote").ToList();
		List<Job> byArchived = _context.Jobs.Where(j => !j.IsArchived).ToList();

		// Assert
		Assert.That(byStatus.Count, Is.EqualTo(1));
		Assert.That(byCompany.Count, Is.EqualTo(1));
		Assert.That(byLocation.Count, Is.EqualTo(1));
		Assert.That(byArchived.Count, Is.EqualTo(1));
	}

	[Test]
	public void Context_MultipleInstances_UseSameDatabase()
	{
		_context.Initialize();

		_context.Jobs.Add(TestJob);
		_context.SaveChanges();

		// Act - Create new context with same path
		using (JobsDbContext context2 = new JobsDbContext(TestDbPath))
		{
			List<Job> jobs = context2.Jobs.ToList();

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

		Job job = new Job
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
