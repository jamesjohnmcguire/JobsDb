/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseReadinessTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

/// <summary>
/// Covers readiness checks and their effects on the database connection.
/// </summary>
[TestFixture]
internal sealed class DatabaseReadinessTests : BaseTestsSupport
{
	/// <summary>
	/// Prepares the database used by the readiness tests.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		DatabaseTestSchema.Create(TestDbPath!);
	}

	/// <summary>
	/// Removes the readiness test database.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		using JobsDbContext cleanupContext = new JobsDbContext(TestDbPath!);
		cleanupContext.Database.EnsureDeleted();
	}

	/// <summary>
	/// Checks that validation does not create a missing database.
	/// </summary>
	/// <param name="useOptions">
	/// Whether to supply the path through EF options.
	/// </param>
	[TestCase(false)]
	[TestCase(true)]
	public void MissingDatabase_IsNotCreated(bool useOptions)
	{
		string databasePath = TestDbPath + ".missing.db";
		DbContextOptionsBuilder<JobsDbContext> options =
			new DbContextOptionsBuilder<JobsDbContext>();
		SqliteConnectionStringBuilder settings =
			new SqliteConnectionStringBuilder();
		settings.DataSource = databasePath;
		options.UseSqlite(settings.ConnectionString);
		JobsDbContext missingContext;

		if (useOptions)
		{
			missingContext = new JobsDbContext(options.Options);
		}
		else
		{
			missingContext = new JobsDbContext(databasePath);
		}

		using (missingContext)
		{
			InvalidOperationException? exception =
				Assert.Throws<InvalidOperationException>(
					() => missingContext.ValidateDatabase());

			Assert.That(exception!.Message, Does.Contain("updateDatabase"));
			Assert.That(exception.Message, Does.Contain(databasePath));
			Assert.That(File.Exists(databasePath), Is.False);
		}
	}

	/// <summary>
	/// Checks that validation does not create a missing parent directory.
	/// </summary>
	[Test]
	public void MissingDirectory_IsNotCreated()
	{
		string directoryPath = Path.Combine(
			Path.GetTempPath(), "JobsDb-missing-" + Guid.NewGuid());
		string databasePath = Path.Combine(directoryPath, "missing.db");
		using JobsDbContext missingContext = new JobsDbContext(databasePath);

		Assert.Throws<InvalidOperationException>(
			() => missingContext.ValidateDatabase());
		Assert.That(Directory.Exists(directoryPath), Is.False);
	}

	/// <summary>
	/// Checks that validation leaves an unprepared database unchanged.
	/// </summary>
	/// <param name="containsUserData">
	/// Whether the database contains an unrelated table.
	/// </param>
	[TestCase(false)]
	[TestCase(true)]
	public void UnpreparedDatabase_IsUnchanged(bool containsUserData)
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		context.Database.EnsureDeleted();
		SqliteConnectionStringBuilder settings =
			new SqliteConnectionStringBuilder();
		settings.DataSource = TestDbPath;
		settings.Pooling = false;

		using (SqliteConnection connection =
			new SqliteConnection(settings.ConnectionString))
		{
			connection.Open();

			if (containsUserData)
			{
				using SqliteCommand command = connection.CreateCommand();
				command.CommandText =
					"CREATE TABLE UserData (value TEXT); " +
					"INSERT INTO UserData VALUES ('Preserve this value');";
				command.ExecuteNonQuery();
			}
		}

		byte[] expectedBytes = ReadDatabaseBytes();

		Assert.Throws<InvalidOperationException>(
			() => context.ValidateDatabase());
		Assert.That(ReadDatabaseBytes(), Is.EqualTo(expectedBytes));
	}

	/// <summary>
	/// Checks that validation rejects an older schema without upgrading it.
	/// </summary>
	[Test]
	public void OlderDatabase_IsNotUpgraded()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		IMigrator migrator = context.GetService<IMigrator>();
		migrator.Migrate("20261116204352_UseCamelCaseColumns");
		string[] expectedVersions =
			context.Database.GetAppliedMigrations().ToArray();
		byte[] expectedBytes = ReadDatabaseBytes();

		InvalidOperationException? exception =
			Assert.Throws<InvalidOperationException>(
				() => context.ValidateDatabase());

		Assert.That(
			exception!.Message,
			Does.Contain("20261116222114_AllowOptionalJobDetails"));
		Assert.That(
			context.Database.GetAppliedMigrations(),
			Is.EqualTo(expectedVersions));
		Assert.That(ReadDatabaseBytes(), Is.EqualTo(expectedBytes));
	}

	/// <summary>
	/// Checks that a newer SQL version with a compatible schema is accepted.
	/// </summary>
	[Test]
	public void NewerSqlVersion_IsAccepted()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		context.Database.ExecuteSqlRaw(
			"INSERT INTO __EFMigrationsHistory VALUES " +
			"('20261117090000_FutureSqlUpgrade', 'sql');");

		Assert.DoesNotThrow(() => context.ValidateDatabase());
		Assert.That(
			context.Database.GetAppliedMigrations(),
			Does.Contain("20261117090000_FutureSqlUpgrade"));
	}

	/// <summary>
	/// Checks that validation reports a missing table without creating it.
	/// </summary>
	[Test]
	public void MissingMappedTable_IsNotRecreated()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		context.Database.ExecuteSqlRaw("DROP TABLE Jobs;");
		byte[] expectedBytes = ReadDatabaseBytes();

		InvalidOperationException? exception =
			Assert.Throws<InvalidOperationException>(
				() => context.ValidateDatabase());

		Assert.That(exception!.Message, Does.Contain("Jobs"));
		Assert.That(ReadDatabaseBytes(), Is.EqualTo(expectedBytes));
	}

	/// <summary>
	/// Checks that validation reports a missing column without adding it.
	/// </summary>
	[Test]
	public void MissingMappedColumn_IsNotAdded()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		context.Database.ExecuteSqlRaw(
			"ALTER TABLE Jobs DROP COLUMN description;");
		byte[] expectedBytes = ReadDatabaseBytes();

		InvalidOperationException? exception =
			Assert.Throws<InvalidOperationException>(
				() => context.ValidateDatabase());

		Assert.That(exception!.Message, Does.Contain("description"));
		Assert.That(ReadDatabaseBytes(), Is.EqualTo(expectedBytes));
	}

	/// <summary>
	/// Checks that validation leaves a current database and its records intact.
	/// </summary>
	[Test]
	public void CurrentDatabase_PreservesSchemaAndData()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		Job job = CopyJob(TestJob!);
		job.Status = ApplicationStatus.Applied;
		job.Notes = "Preserve these notes";
		context.Jobs.Add(job);
		context.SaveChanges();
		context.ChangeTracker.Clear();
		byte[] expectedBytes = ReadDatabaseBytes();

		context.ValidateDatabase();
		context.ValidateDatabase();

		Assert.That(ReadDatabaseBytes(), Is.EqualTo(expectedBytes));
		Assert.That(
			context.Jobs.Single(), Is.EqualTo(job).UsingPropertiesComparer());
	}

	/// <summary>
	/// Checks that validation leaves a previously closed connection closed.
	/// </summary>
	[Test]
	public void ClosedContextConnection_RemainsClosed()
	{
		using JobsDbContext context = new JobsDbContext(TestDbPath!);
		DbConnection connection = context.Database.GetDbConnection();

		context.ValidateDatabase();

		Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
	}

	/// <summary>
	/// Checks that validation leaves a caller-owned connection open.
	/// </summary>
	/// <param name="prepareDatabase">
	/// Whether to prepare the caller's database before validation.
	/// </param>
	[TestCase(false)]
	[TestCase(true)]
	public void CallerConnection_RemainsOpen(bool prepareDatabase)
	{
		using SqliteConnection connection =
			new SqliteConnection("Data Source=:memory:");
		connection.Open();

		if (prepareDatabase)
		{
			DatabaseTestSchema.Load(connection);
		}

		DbContextOptionsBuilder<JobsDbContext> options =
			new DbContextOptionsBuilder<JobsDbContext>();
		options.UseSqlite(connection);
		using JobsDbContext sharedContext = new JobsDbContext(options.Options);

		if (prepareDatabase)
		{
			Assert.DoesNotThrow(() => sharedContext.ValidateDatabase());
		}
		else
		{
			Assert.Throws<InvalidOperationException>(
				() => sharedContext.ValidateDatabase());
		}

		Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
	}

	private byte[] ReadDatabaseBytes()
	{
		using FileStream databaseFile = new FileStream(
			TestDbPath!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using MemoryStream contents = new MemoryStream();
		databaseFile.CopyTo(contents);
		return contents.ToArray();
	}
}
