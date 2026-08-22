/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbContext.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core DbContext for JobsDb application
/// Manages database connection and entity configurations.
/// </summary>
public class JobsDbContext : DbContext
{
	public DbSet<Job> Jobs { get; set; }

	public DbSet<ScraperCredential> Credentials { get; set; }

	public DbSet<ScraperLog> ScraperLogs { get; set; }

	public DbSet<SearchFilter> SearchFilters { get; set; }

	private readonly string dbPath;

	/// <summary>
	/// Default constructor - creates database in LocalApplicationData folder.
	/// </summary>
	public JobsDbContext()
	{
		var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var appFolder = Path.Combine(folder, "JobsDb");
		Directory.CreateDirectory(appFolder);
		dbPath = Path.Combine(appFolder, "jobsdb.db");
	}

	/// <summary>
	/// Constructor with custom database path.
	/// </summary>
	public JobsDbContext(string dbPath)
	{
		this.dbPath = dbPath;
		var directory = Path.GetDirectoryName(this.dbPath);
		if (!string.IsNullOrEmpty(directory))
		{
			Directory.CreateDirectory(directory);
		}
	}

	/// <summary>
	/// Constructor for dependency injection.
	/// </summary>
	public JobsDbContext(DbContextOptions<JobsDbContext> options)
		: base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSqlite($"Data Source={dbPath}");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Each entity's configuration lives in its own IEntityTypeConfiguration<T>
		// class under Data/Configurations/. This picks up every one of them from
		// this assembly automatically - no per-entity wiring needed here.
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsDbContext).Assembly);
	}


	/// <summary>
	/// <summary>
	/// Initialize the database by applying any pending migrations.
	/// Creates the database file if it doesn't exist yet, and brings
	/// an existing database up to date with the current model.
	/// </summary>
	public void Initialize()
	{
		Database.Migrate();
		Console.WriteLine($"✓ Database initialized/migrated at: {dbPath}");
	}

	/// <summary>
	/// Get the database file path.
	/// </summary>
	public string GetDatabasePath()
	{
		return dbPath;
	}
}
