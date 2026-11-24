/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContext.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
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

	/// <summary>
	/// Gets or sets the mapped job listings.
	/// </summary>
	public DbSet<Job> Jobs { get; set; }

	/// <summary>
	/// Gets or sets the mapped scraper credentials.
	/// </summary>
	public DbSet<ScraperCredential> Credentials { get; set; }

	/// <summary>
	/// Gets or sets the mapped acquisition logs.
	/// </summary>
	public DbSet<ScraperLog> ScraperLogs { get; set; }

	/// <summary>
	/// Gets or sets the mapped saved search filters.
	/// </summary>
	public DbSet<SearchFilter> SearchFilters { get; set; }

	/// <summary>
	/// Checks that SQL upgrades and mapped queries are ready without changing
	/// the database schema or creating a missing database.
	/// </summary>
	public void ValidateDatabase()
	{
		DbConnection connection = Database.GetDbConnection();

		try
		{
			if (connection.State == ConnectionState.Open)
			{
				ValidateUpgradeHistory(connection);
			}
			else
			{
				SqliteConnectionStringBuilder settings =
					new SqliteConnectionStringBuilder(
						connection.ConnectionString);
				settings.Pooling = false;

				if (settings.Mode != SqliteOpenMode.Memory &&
					settings.DataSource != ":memory:")
				{
					settings.Mode = SqliteOpenMode.ReadOnly;
				}

				using SqliteConnection validationConnection =
					new SqliteConnection(settings.ConnectionString);
				validationConnection.Open();
				ValidateUpgradeHistory(validationConnection);
			}

			// Zero-row queries check mapped columns without loading user data.
			Jobs.AsNoTracking().Take(0).Load();
			Credentials.AsNoTracking().Take(0).Load();
			ScraperLogs.AsNoTracking().Take(0).Load();
			SearchFilters.AsNoTracking().Take(0).Load();
		}
		catch (Exception exception) when (
			exception is SqliteException or InvalidDataException)
		{
			string databasePath = GetDatabasePath();
			string message =
				$"Database '{databasePath}' is not ready. " +
				exception.Message + " Run DevelopmentTools/" +
				"updateDatabase.cmd or updateDatabase.sh for this database " +
				"before starting the app.";
			throw new InvalidOperationException(message, exception);
		}
	}

	/// <summary>
	/// Gets the configured SQLite data source, including in-memory names.
	/// </summary>
	/// <returns>The SQLite connection's data source.</returns>
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

	/// <inheritdoc/>
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

	private static void ValidateUpgradeHistory(DbConnection connection)
	{
		using DbCommand command = connection.CreateCommand();
		command.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory";
		HashSet<string> appliedVersions =
			new HashSet<string>(StringComparer.Ordinal);

		using (DbDataReader reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
				string version = reader.GetString(0);
				appliedVersions.Add(version);
			}
		}

		Stream? manifest = typeof(JobsDbContext).Assembly
			.GetManifestResourceStream("JobsDb.UpgradeOrder");

		if (manifest == null)
		{
			throw new InvalidDataException("The SQL upgrade list is missing.");
		}

		using StreamReader manifestReader = new StreamReader(manifest);
		string? entry = manifestReader.ReadLine();

		while (entry != null)
		{
			string[] fields = entry.Split(
				' ', StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length != 2)
			{
				throw new InvalidDataException(
					"The SQL upgrade list is invalid.");
			}

			string version = fields[0];

			if (!appliedVersions.Contains(version))
			{
				throw new InvalidDataException(
					$"Required SQL upgrade '{version}' has not been applied.");
			}

			entry = manifestReader.ReadLine();
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
		}

		SqliteConnectionStringBuilder connectionStringBuilder =
			new SqliteConnectionStringBuilder();
		connectionStringBuilder.DataSource = dataSource;

		if (databasePath != ":memory:")
		{
			connectionStringBuilder.Mode = SqliteOpenMode.ReadWrite;
		}

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
