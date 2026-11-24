/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabasePathTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Covers database source resolution and reporting.
/// </summary>
[TestFixture]
internal sealed class DatabasePathTests
{
	private string databaseDirectory = string.Empty;

	/// <summary>
	/// Creates a directory for temporary database paths.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		string directoryName = "JobsDb-path-tests-" + Guid.NewGuid();
		databaseDirectory = Path.Combine(Path.GetTempPath(), directoryName);
		Directory.CreateDirectory(databaseDirectory);
	}

	/// <summary>
	/// Removes the temporary database directory.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		Directory.Delete(databaseDirectory, true);
	}

	/// <summary>
	/// Checks that design-time and runtime contexts use the same default path.
	/// </summary>
	[Test]
	public void Factory_UsesRuntimeDatabasePath()
	{
		using JobsDbContext runtimeContext = new JobsDbContext();
		JobsDbContextFactory factory = new JobsDbContextFactory();
		using JobsDbContext toolingContext =
			factory.CreateDbContext(Array.Empty<string>());

		Assert.That(
			toolingContext.GetDatabasePath(),
			Is.EqualTo(runtimeContext.GetDatabasePath()));
	}

	/// <summary>
	/// Checks that a relative filename resolves to an absolute path.
	/// </summary>
	[Test]
	public void Constructor_RelativePath_ResolvesAbsolutePath()
	{
		string expectedPath = Path.Combine(databaseDirectory, "relative.db");
		string relativePath = Path.GetRelativePath(
			Environment.CurrentDirectory, expectedPath);
		using JobsDbContext context = new JobsDbContext(relativePath);

		Assert.That(context.GetDatabasePath(), Is.EqualTo(expectedPath));
	}

	/// <summary>
	/// Checks that a semicolon remains part of the SQLite filename.
	/// </summary>
	[Test]
	public void Constructor_SemicolonInPath_CreatesLiteralFilename()
	{
		string databasePath = Path.Combine(
			databaseDirectory, "jobs;extra setting.db");
		DatabaseTestSchema.Create(databasePath);
		using JobsDbContext context = new JobsDbContext(databasePath);

		try
		{
			context.ValidateDatabase();

			Assert.That(context.GetDatabasePath(), Is.EqualTo(databasePath));
			Assert.That(File.Exists(databasePath), Is.True);
		}
		finally
		{
			context.Database.EnsureDeleted();
		}
	}

	/// <summary>
	/// Checks that context options supply the reported database path.
	/// </summary>
	[Test]
	public void OptionsConstructor_UsesConfiguredDatabasePath()
	{
		string databasePath = Path.Combine(databaseDirectory, "options.db");
		SqliteConnectionStringBuilder connectionStringBuilder =
			new SqliteConnectionStringBuilder();
		connectionStringBuilder.DataSource = databasePath;
		DbContextOptionsBuilder<JobsDbContext> optionsBuilder =
			new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);
		DatabaseTestSchema.Create(databasePath);
		using JobsDbContext context = new JobsDbContext(optionsBuilder.Options);

		try
		{
			context.ValidateDatabase();

			Assert.That(context.GetDatabasePath(), Is.EqualTo(databasePath));
			Assert.That(File.Exists(databasePath), Is.True);
		}
		finally
		{
			context.Database.EnsureDeleted();
		}
	}

	/// <summary>
	/// Checks that path reporting follows a connection string override.
	/// </summary>
	[Test]
	public void GetDatabasePath_ReflectsConnectionOverride()
	{
		string originalPath = Path.Combine(databaseDirectory, "original.db");
		string replacementPath = Path.Combine(
			databaseDirectory, "replacement.db");
		DatabaseTestSchema.Create(replacementPath);
		using JobsDbContext context = new JobsDbContext(originalPath);
		SqliteConnectionStringBuilder connectionStringBuilder =
			new SqliteConnectionStringBuilder();
		connectionStringBuilder.DataSource = replacementPath;
		context.Database.SetConnectionString(
			connectionStringBuilder.ConnectionString);

		try
		{
			context.ValidateDatabase();

			Assert.That(
				context.GetDatabasePath(), Is.EqualTo(replacementPath));
			Assert.That(File.Exists(replacementPath), Is.True);
			Assert.That(File.Exists(originalPath), Is.False);
		}
		finally
		{
			context.Database.EnsureDeleted();
		}
	}

	/// <summary>
	/// Checks that the path constructor reports the in-memory source.
	/// </summary>
	[Test]
	public void Constructor_InMemoryDatabase_ReportsMemorySource()
	{
		using JobsDbContext context = new JobsDbContext(":memory:");
		context.Database.OpenConnection();

		Assert.That(context.GetDatabasePath(), Is.EqualTo(":memory:"));
	}

	/// <summary>
	/// Checks that the options constructor reports the in-memory source.
	/// </summary>
	[Test]
	public void OptionsConstructor_InMemoryDatabase_ReportsMemorySource()
	{
		DbContextOptionsBuilder<JobsDbContext> optionsBuilder =
			new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite("Data Source=:memory:");
		using JobsDbContext context = new JobsDbContext(optionsBuilder.Options);
		context.Database.OpenConnection();

		Assert.That(context.GetDatabasePath(), Is.EqualTo(":memory:"));
	}

	/// <summary>
	/// Checks that a blank database path is rejected.
	/// </summary>
	/// <param name="databasePath">
	/// The blank path passed to the constructor.
	/// </param>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("\t")]
	public void Constructor_RejectsBlankPath(string databasePath)
	{
		Assert.Throws<ArgumentException>(() =>
		{
			using JobsDbContext context = new JobsDbContext(databasePath);
		});
	}

	/// <summary>
	/// Checks that a null database path is rejected.
	/// </summary>
	[Test]
	public void Constructor_RejectsNullPath()
	{
		string? databasePath = null;

		Assert.Throws<ArgumentNullException>(() =>
		{
			using JobsDbContext context = new JobsDbContext(databasePath!);
		});
	}
}
