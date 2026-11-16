/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContext.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.Data.Common;
using System.IO;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Entity Framework Core DbContext for JobsDb application
/// Manages database connection and entity configurations.
/// </summary>
public class JobsDbContext : DbContext
{
	/// <summary>
	/// Initializes a context using the default application database.
	/// </summary>
	public JobsDbContext()
		: this(GetDefaultDatabasePath())
	{
	}

	/// <summary>
	/// Initializes a context using the specified SQLite database path.
	/// </summary>
	/// <param name="databasePath">A file path or :memory:.</param>
	public JobsDbContext(string databasePath)
		: this(CreateOptions(databasePath))
	{
	}

	/// <summary>
	/// Initializes a context using the supplied database options.
	/// </summary>
	/// <param name="options">The configured database options.</param>
	public JobsDbContext(DbContextOptions<JobsDbContext> options)
		: base(options)
	{
	}

	public DbSet<Job> Jobs { get; set; }

	public DbSet<ScraperCredential> Credentials { get; set; }

	public DbSet<ScraperLog> ScraperLogs { get; set; }

	public DbSet<SearchFilter> SearchFilters { get; set; }

	/// <summary>
	/// Initialize the database by applying any pending migrations.
	/// Creates the database file if it doesn't exist yet, and brings
	/// an existing database up to date with the current model.
	/// </summary>
	public void Initialize()
	{
		Database.Migrate();
		string databasePath = GetDatabasePath();
		Console.WriteLine($"Database initialized/migrated at: {databasePath}");
	}

	/// <summary>
	/// Gets the configured SQLite data source, including in-memory names.
	/// </summary>
	public string GetDatabasePath()
	{
		DbConnection connection = Database.GetDbConnection();
		string dataSource = connection.DataSource;

		if (!string.IsNullOrEmpty(dataSource))
		{
			return dataSource;
		}

		SqliteConnectionStringBuilder connectionStringBuilder =
			new SqliteConnectionStringBuilder(connection.ConnectionString);
		return connectionStringBuilder.DataSource;
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		ArgumentNullException.ThrowIfNull(modelBuilder);

		base.OnModelCreating(modelBuilder);

		// Apply entity mappings before assigning database column names.
		modelBuilder.ApplyConfigurationsFromAssembly(
			typeof(JobsDbContext).Assembly);

		foreach (IMutableEntityType entityType in
			modelBuilder.Model.GetEntityTypes())
		{
			foreach (IMutableProperty property in entityType.GetProperties())
			{
				string propertyName = property.Name;
				char firstCharacter = char.ToLowerInvariant(propertyName[0]);
				string columnName = firstCharacter + propertyName.Substring(1);
				property.SetColumnName(columnName);
			}
		}
	}

	private static DbContextOptions<JobsDbContext> CreateOptions(
		string databasePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

		string dataSource = databasePath;

		if (databasePath != ":memory:")
		{
			dataSource = Path.GetFullPath(databasePath);
			string? databaseDirectory = Path.GetDirectoryName(dataSource);

			if (databaseDirectory != null)
			{
				Directory.CreateDirectory(databaseDirectory);
			}
		}

		SqliteConnectionStringBuilder connectionStringBuilder =
			new SqliteConnectionStringBuilder();
		connectionStringBuilder.DataSource = dataSource;

		DbContextOptionsBuilder<JobsDbContext> optionsBuilder =
			new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);
		return optionsBuilder.Options;
	}

	private static string GetDefaultDatabasePath()
	{
		string applicationDataFolder = Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData);
		string applicationFolder = Path.Combine(
			applicationDataFolder, "JobsDb");
		return Path.Combine(applicationFolder, "jobsdb.db");
	}
}
