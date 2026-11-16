/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContextTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NUnit.Framework;

[TestFixture]
internal sealed class JobsDbContextTests : BaseTestsSupport
{
	private JobsDbContext context;

	[SetUp]
	public void SetUp()
	{
		context = new JobsDbContext(TestDbPath);
	}

	[TearDown]
	public void TearDown()
	{
		context.Database.EnsureDeleted();
		context.Dispose();
	}

	[Test]
	public void Initialize_CreatesDatabase()
	{
		// Act
		context.Initialize();

		// Assert
		Assert.That(File.Exists(TestDbPath), Is.True);
		Assert.That(context.Database.CanConnect(), Is.True);
	}

	[Test]
	public void GetDatabasePath_ReturnsCorrectPath()
	{
		// Act
		var path = context.GetDatabasePath();

		// Assert
		Assert.That(path, Is.EqualTo(TestDbPath));
	}

	[Test]
	public void Jobs_DbSet_IsNotNull()
	{
		// Act
		context.Initialize();

		// Assert
		Assert.That(context.Jobs, Is.Not.Null);
	}

	[Test]
	public void Credentials_DbSet_IsNotNull()
	{
		// Act
		context.Initialize();

		// Assert
		Assert.That(context.Credentials, Is.Not.Null);
	}

	[Test]
	public void ScraperLogs_DbSet_IsNotNull()
	{
		// Act
		context.Initialize();

		// Assert
		Assert.That(context.ScraperLogs, Is.Not.Null);
	}

	[Test]
	public void SearchFilters_DbSet_IsNotNull()
	{
		// Act
		context.Initialize();

		// Assert
		Assert.That(context.SearchFilters, Is.Not.Null);
	}

	[TestCase("Test", "123", true)]
	[TestCase("AnotherSource", "123", false)]
	[TestCase("Test", "456", false)]
	public void Job_SourceIdentity_EnforcesCompositeUniqueness(
		string source,
		string sourceJobIdentifier,
		bool expectsDuplicate)
	{
		context.Initialize();

		Job originalJob = CopyJob(TestJob!);
		Job incomingJob = CopyJob(originalJob);
		incomingJob.Title = "Different Job";
		incomingJob.Company = "Company B";
		incomingJob.Source = source;
		incomingJob.SourceJobId = sourceJobIdentifier;
		incomingJob.SourceUrl = "https://test.com/2";

		context.Jobs.Add(originalJob);
		context.SaveChanges();

		context.Jobs.Add(incomingJob);

		if (expectsDuplicate)
		{
			DbUpdateException? updateException =
				Assert.Throws<DbUpdateException>(() => context.SaveChanges());
			SqliteException? databaseException =
				updateException?.InnerException as SqliteException;

			Assert.That(databaseException, Is.Not.Null);
			Assert.That(
				databaseException!.SqliteExtendedErrorCode,
				Is.EqualTo(2067)); // SQLITE_CONSTRAINT_UNIQUE
			Assert.That(context.Jobs.Count(), Is.EqualTo(1));
		}
		else
		{
			context.SaveChanges();

			Assert.That(incomingJob.Id, Is.Not.EqualTo(originalJob.Id));
			Assert.That(context.Jobs.Count(), Is.EqualTo(2));
		}
	}

	[TestCase("LinkedIn", true)]
	[TestCase("TokyoDev", false)]
	public void ScraperCredential_Source_EnforcesUniqueness(
		string source,
		bool expectsDuplicate)
	{
		context.Initialize();

		ScraperCredential originalCredential = new ScraperCredential
		{
			CookieData = "original cookie",
			Source = "LinkedIn",
			Username = "user1@example.com",
			EncryptedPassword = "encrypted1",
			IsActive = true
		};

		ScraperCredential incomingCredential = new ScraperCredential
		{
			CookieData = "incoming cookie",
			Source = source,
			Username = "user2@example.com",
			EncryptedPassword = "encrypted2",
			IsActive = true
		};

		context.Credentials.Add(originalCredential);
		context.SaveChanges();

		context.Credentials.Add(incomingCredential);

		if (expectsDuplicate)
		{
			DbUpdateException? updateException =
				Assert.Throws<DbUpdateException>(() => context.SaveChanges());
			SqliteException? databaseException =
				updateException?.InnerException as SqliteException;

			Assert.That(databaseException, Is.Not.Null);
			Assert.That(
				databaseException!.SqliteExtendedErrorCode,
				Is.EqualTo(2067)); // SQLITE_CONSTRAINT_UNIQUE
			Assert.That(context.Credentials.Count(), Is.EqualTo(1));
		}
		else
		{
			context.SaveChanges();

			Assert.That(
				incomingCredential.Id,
				Is.Not.EqualTo(originalCredential.Id));
			Assert.That(context.Credentials.Count(), Is.EqualTo(2));
		}
	}

	[Test]
	public void Job_StatusEnum_SavesAsString()
	{
		context.Initialize();

		Job appliedJob = CopyJob(TestJob!);
		appliedJob.Status = ApplicationStatus.Applied;
		context.Jobs.Add(appliedJob);
		context.SaveChanges();

		using SqliteConnection databaseConnection = new SqliteConnection(
			context.Database.GetConnectionString());
		databaseConnection.Open();

		using SqliteCommand statusCommand = databaseConnection.CreateCommand();
		statusCommand.CommandText =
			"SELECT status, typeof(status) FROM Jobs WHERE id = $jobIdentifier";
		statusCommand.Parameters.AddWithValue("$jobIdentifier", appliedJob.Id);

		using SqliteDataReader statusReader = statusCommand.ExecuteReader();

		Assert.That(statusReader.Read(), Is.True);
		Assert.That(statusReader.GetString(0), Is.EqualTo("Applied"));
		Assert.That(statusReader.GetString(1), Is.EqualTo("text"));
	}

	[Test]
	public void Job_DateScraped_HasDefaultValue()
	{
		context.Initialize();

		context.Jobs.Add(TestJob);
		context.SaveChanges();

		var saved = context.Jobs.First();

		// Assert
		Assert.That(saved.DateScraped, Is.Not.EqualTo(default(DateTime)));
	}

	[Test]
	public void SearchFilter_CreatedDate_HasDefaultValue()
	{
		// Arrange
		context.Initialize();

		SearchFilter filter = new SearchFilter
		{
			Name = "Test Filter",
			Keywords = "developer",
			IsActive = true,
			Location = "Remote",
			Source = "TestSource"
		};

		// Act
		context.SearchFilters.Add(filter);
		context.SaveChanges();

		var saved = context.SearchFilters.First();

		// Assert
		Assert.That(saved.CreatedDate, Is.Not.EqualTo(default(DateTime)));
	}

	[Test]
	public void ScraperCredential_IsActive_HasDefaultValue()
	{
		// Arrange
		context.Initialize();

		ScraperCredential cred = new ScraperCredential
		{
			CookieData = "cookie",
			IsActive = true,
			Source = "TestSource",
			Username = "test@example.com",
			EncryptedPassword = "encrypted"
		};

		// Act
		context.Credentials.Add(cred);
		context.SaveChanges();

		var saved = context.Credentials.First();

		// Assert
		Assert.That(saved.IsActive, Is.True);
	}

	[TestCase("IX_Jobs_company", false, new[] { "company" })]
	[TestCase("IX_Jobs_datePosted", false, new[] { "datePosted" })]
	[TestCase("IX_Jobs_isArchived", false, new[] { "isArchived" })]
	[TestCase("IX_Jobs_location", false, new[] { "location" })]
	[TestCase(
		"IX_Jobs_source_sourceJobId", true, new[] { "source", "sourceJobId" })]
	[TestCase("IX_Jobs_sourceJobId", false, new[] { "sourceJobId" })]
	[TestCase("IX_Jobs_status", false, new[] { "status" })]
	public void Job_Indexes_HaveExpectedDefinitions(
		string indexName,
		bool expectsUnique,
		string[] expectedColumns)
	{
		context.Initialize();

		using SqliteConnection databaseConnection = new SqliteConnection(
			context.Database.GetConnectionString());
		databaseConnection.Open();

		using SqliteCommand indexCommand = databaseConnection.CreateCommand();
		indexCommand.CommandText = "SELECT [unique], partial " +
			"FROM pragma_index_list('Jobs') WHERE name = $indexName";
		indexCommand.Parameters.AddWithValue("$indexName", indexName);

		using (SqliteDataReader indexReader = indexCommand.ExecuteReader())
		{
			Assert.That(indexReader.Read(), Is.True);
			Assert.That(indexReader.GetBoolean(0), Is.EqualTo(expectsUnique));
			Assert.That(indexReader.GetBoolean(1), Is.False);
		}

		using SqliteCommand columnsCommand =
			databaseConnection.CreateCommand();
		columnsCommand.CommandText =
			"SELECT name FROM pragma_index_info($indexName) ORDER BY seqno";
		columnsCommand.Parameters.AddWithValue("$indexName", indexName);

		using SqliteDataReader columnsReader = columnsCommand.ExecuteReader();
		List<string> actualColumns = new List<string>();

		while (columnsReader.Read())
		{
			string columnName = columnsReader.GetString(0);
			actualColumns.Add(columnName);
		}

		Assert.That(actualColumns, Is.EqualTo(expectedColumns));
	}

	[Test]
	public void Context_MultipleInstances_UseSameDatabase()
	{
		context.Initialize();

		context.Jobs.Add(TestJob);
		context.SaveChanges();

		// Act - Create new context with same path
		using (JobsDbContext context2 = new JobsDbContext(TestDbPath))
		{
			List<Job> jobs = context2.Jobs.ToList();

			// Assert
			Assert.That(jobs.Count, Is.EqualTo(1));
			Assert.That(jobs[0].Title, Is.EqualTo("Test"));
		}
	}

	[TestCase(nameof(Job.Title), "title")]
	[TestCase(nameof(Job.Company), "company")]
	[TestCase(nameof(Job.Source), "source")]
	[TestCase(nameof(Job.SourceUrl), "sourceUrl")]
	[TestCase(nameof(Job.SourceJobId), "sourceJobId")]
	public void Job_RequiredFields_RejectNull(
		string propertyName,
		string columnName)
	{
		context.Initialize();

		Job job = CopyJob(TestJob!);
		context.Jobs.Add(job);
		EntityEntry jobEntry = context.Entry(job);
		PropertyEntry propertyEntry = jobEntry.Property(propertyName);
		propertyEntry.CurrentValue = null;

		DbUpdateException? updateException =
			Assert.Throws<DbUpdateException>(() => context.SaveChanges());
		SqliteException? databaseException =
			updateException?.InnerException as SqliteException;

		Assert.That(databaseException, Is.Not.Null);
		Assert.That(
			databaseException!.SqliteExtendedErrorCode,
			Is.EqualTo(1299)); // SQLITE_CONSTRAINT_NOTNULL
		Assert.That(
			databaseException.Message,
			Does.Contain("Jobs." + columnName));
	}
}
