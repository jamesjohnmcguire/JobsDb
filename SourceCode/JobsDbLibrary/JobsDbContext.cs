namespace JobsDb.Core.Data;

using JobsDb.Core.Models;
using JobsDb.Core.Scrapers;
using JobsDbLibrary.Scrapers;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

/// <summary>
/// Entity Framework Core DbContext for JobsDb application
/// Manages database connection and entity configurations
/// </summary>
public class JobsDbContext : DbContext
{
	public DbSet<Job> Jobs { get; set; }
	public DbSet<ScraperCredential> Credentials { get; set; }
	public DbSet<ScraperLog> ScraperLogs { get; set; }
	public DbSet<SearchFilter> SearchFilters { get; set; }

	private readonly string _dbPath;

	/// <summary>
	/// Default constructor - creates database in LocalApplicationData folder
	/// </summary>
	public JobsDbContext()
	{
		var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var appFolder = Path.Combine(folder, "JobsDb");
		Directory.CreateDirectory(appFolder);
		_dbPath = Path.Combine(appFolder, "jobsdb.db");
	}

	/// <summary>
	/// Constructor with custom database path
	/// </summary>
	public JobsDbContext(string dbPath)
	{
		_dbPath = dbPath;
		var directory = Path.GetDirectoryName(_dbPath);
		if (!string.IsNullOrEmpty(directory))
		{
			Directory.CreateDirectory(directory);
		}
	}

	/// <summary>
	/// Constructor for dependency injection
	/// </summary>
	public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSqlite($"Data Source={_dbPath}");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// ==================== JOB ENTITY CONFIGURATION ====================
		modelBuilder.Entity<Job>(entity =>
		{
			// Indexes for better query performance
			entity.HasIndex(e => e.SourceJobId);
			entity.HasIndex(e => new { e.Source, e.SourceJobId }).IsUnique();
			entity.HasIndex(e => e.Status);
			entity.HasIndex(e => e.DatePosted);
			entity.HasIndex(e => e.IsArchived);
			entity.HasIndex(e => e.Company);
			entity.HasIndex(e => e.Location);

			// Enum stored as string for readability
			entity.Property(e => e.Status)
				.HasConversion<string>();

			// Default value for DateScraped
			entity.Property(e => e.DateScraped)
				.HasDefaultValueSql("datetime('now')");

			// Column constraints
			entity.Property(e => e.Title)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(e => e.Company)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(e => e.Source)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(e => e.SourceUrl)
				.IsRequired()
				.HasMaxLength(1000);

			entity.Property(e => e.SourceJobId)
				.HasMaxLength(200);

			entity.Property(e => e.Location)
				.HasMaxLength(200);

			entity.Property(e => e.JobType)
				.HasMaxLength(50);

			entity.Property(e => e.RemoteType)
				.HasMaxLength(50);

			entity.Property(e => e.SalaryCurrency)
				.HasMaxLength(10);
		});

		// ==================== SCRAPER CREDENTIAL CONFIGURATION ====================
		modelBuilder.Entity<ScraperCredential>(entity =>
		{
			entity.HasIndex(e => e.Source).IsUnique();

			entity.Property(e => e.IsActive)
				.HasDefaultValue(true);

			entity.Property(e => e.Source)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(e => e.Username)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(e => e.EncryptedPassword)
				.IsRequired();
		});

		// ==================== SCRAPER LOG CONFIGURATION ====================
		modelBuilder.Entity<ScraperLog>(entity =>
		{
			entity.HasIndex(e => e.Timestamp);
			entity.HasIndex(e => e.Source);
			entity.HasIndex(e => e.Success);

			entity.Property(e => e.Source)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(e => e.Timestamp)
				.HasDefaultValueSql("datetime('now')");
		});

		// ==================== SEARCH FILTER CONFIGURATION ====================
		modelBuilder.Entity<SearchFilter>(entity =>
		{
			entity.HasIndex(e => e.IsActive);
			entity.HasIndex(e => e.Name);

			entity.Property(e => e.IsActive)
				.HasDefaultValue(true);

			entity.Property(e => e.CreatedDate)
				.HasDefaultValueSql("datetime('now')");

			entity.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.Source)
				.HasMaxLength(50);

			entity.Property(e => e.Keywords)
				.HasMaxLength(500);

			entity.Property(e => e.Location)
				.HasMaxLength(200);
		});
	}

	/// <summary>
	/// Initialize the database (create if doesn't exist)
	/// </summary>
	public void Initialize()
	{
		Database.EnsureCreated();
		Console.WriteLine($"✓ Database initialized at: {_dbPath}");
	}

	/// <summary>
	/// Get the database file path
	/// </summary>
	public string GetDatabasePath()
	{
		return _dbPath;
	}
}
