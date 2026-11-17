/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseSchemaTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Persistence;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;
using StorageContext = DigitalZenWorks.JobsDb.Library.Persistence.Generated.
	JobStorageContext;
using StorageJob = DigitalZenWorks.JobsDb.Library.Persistence.Generated.Job;

[TestFixture]
internal sealed class DatabaseSchemaTests : BaseTestsSupport
{
	[Test]
	public void Schema_ValidateDatabasePreservesExistingData()
	{
		using JobsDbContext schemaContext = CreateContext();
		LoadSchema(schemaContext);

		SearchFilter searchFilter = new SearchFilter
		{
			Name = "Preserved Filter",
			Keywords = "developer",
			Location = "Remote",
			Source = "Test",
			IsActive = true
		};
		schemaContext.SearchFilters.Add(searchFilter);
		schemaContext.SaveChanges();
		schemaContext.ChangeTracker.Clear();

		Assert.That(schemaContext.Database.GetPendingMigrations(), Is.Empty);

		schemaContext.ValidateDatabase();
		schemaContext.ValidateDatabase();

		List<SearchFilter> savedFilters =
			schemaContext.SearchFilters.AsNoTracking().ToList();

		Assert.That(savedFilters, Has.Count.EqualTo(1));
		Assert.That(savedFilters[0].Name, Is.EqualTo("Preserved Filter"));
	}

	[TestCase("Jobs")]
	[TestCase("Credentials")]
	[TestCase("ScraperLogs")]
	[TestCase("SearchFilters")]
	public void ApplicationColumns_UseCamelCase(string tableName)
	{
		using JobsDbContext context = CreateContext();
		LoadSchema(context);

		DbConnection connection = context.Database.GetDbConnection();
		using DbCommand command = connection.CreateCommand();
		command.CommandText =
			"SELECT name FROM pragma_table_info($tableName)";
		DbParameter tableParameter = command.CreateParameter();
		tableParameter.ParameterName = "$tableName";
		tableParameter.Value = tableName;
		command.Parameters.Add(tableParameter);

		using DbDataReader reader = command.ExecuteReader();
		List<string> columnNames = new List<string>();

		while (reader.Read())
		{
			string columnName = reader.GetString(0);
			columnNames.Add(columnName);
			bool startsWithLowercase = char.IsLower(columnName[0]);
			Assert.That(startsWithLowercase, Is.True, columnName);
		}

		Assert.That(columnNames, Is.Not.Empty);
	}

	[Test]
	public void CamelCaseMigration_PreservesExistingRecords()
	{
		using JobsDbContext context = CreateContext();
		IMigrator migrator = context.GetService<IMigrator>();
		migrator.Migrate("20260723233159_InitialCreate");

		Job existingJob = CopyJob(TestJob!);
		existingJob.Notes = "Preserve application notes";
		SearchFilter existingFilter = new SearchFilter
		{
			Name = "Preserved Filter",
			Keywords = "developer",
			Location = "Remote",
			Source = "Test",
			IsActive = true
		};
		ScraperCredential existingCredential = new ScraperCredential
		{
			Source = "Test",
			Username = "test@example.com",
			EncryptedPassword = "test-encrypted-value",
			CookieData = "test cookie",
			IsActive = true
		};
		ScraperLog existingLog = new ScraperLog
		{
			Source = "Test",
			Timestamp = DateTime.UtcNow,
			JobsFound = 1,
			Success = true,
			ErrorMessage = string.Empty
		};
		context.Jobs.Add(existingJob);
		context.SearchFilters.Add(existingFilter);
		context.Credentials.Add(existingCredential);
		context.ScraperLogs.Add(existingLog);
		context.SaveChanges();
		context.ChangeTracker.Clear();

		context.Database.Migrate();

		Job savedJob = context.Jobs.Single();
		SearchFilter savedFilter = context.SearchFilters.Single();
		ScraperCredential savedCredential = context.Credentials.Single();
		ScraperLog savedLog = context.ScraperLogs.Single();

		Assert.That(savedJob.Id, Is.EqualTo(existingJob.Id));
		Assert.That(savedJob.Notes, Is.EqualTo(existingJob.Notes));
		Assert.That(savedFilter.Name, Is.EqualTo(existingFilter.Name));
		Assert.That(
			savedCredential.EncryptedPassword,
			Is.EqualTo(existingCredential.EncryptedPassword));
		Assert.That(savedLog.JobsFound, Is.EqualTo(existingLog.JobsFound));
		Assert.That(context.Database.GetPendingMigrations(), Is.Empty);
	}

	[TestCase(false)]
	[TestCase(true)]
	public void Job_MissingOptionalDetails_SavesNullValues(
		bool useSchemaScript)
	{
		using JobsDbContext context = CreateContext();

		if (useSchemaScript)
		{
			LoadSchema(context);
		}
		else
		{
			context.Database.Migrate();
		}

		Job job = new Job
		{
			Title = "Incomplete Listing",
			Company = "Test Company",
			Source = "Test",
			SourceUrl = "https://example.com/jobs/incomplete",
			SourceJobId = "incomplete",
			DatePosted = DateTime.UtcNow
		};
		context.Jobs.Add(job);
		context.SaveChanges();
		context.ChangeTracker.Clear();

		Job savedJob = context.Jobs.Single();

		Assert.That(savedJob.Location, Is.Null);
		Assert.That(savedJob.Description, Is.Null);
		Assert.That(savedJob.Requirements, Is.Null);
		Assert.That(savedJob.SalaryCurrency, Is.Null);
		Assert.That(savedJob.JobType, Is.Null);
		Assert.That(savedJob.RemoteType, Is.Null);
		Assert.That(savedJob.Notes, Is.Null);
	}

	[Test]
	public void OptionalDetailsMigration_PreservesRecordsInBothDirections()
	{
		using JobsDbContext context = CreateContext();
		IMigrator migrator = context.GetService<IMigrator>();
		migrator.Migrate("20261116204352_UseCamelCaseColumns");

		Job existingJob = CopyJob(TestJob!);
		existingJob.Id = 42;
		existingJob.Status = ApplicationStatus.Applied;
		existingJob.Notes = "Keep these application notes";
		context.Jobs.Add(existingJob);
		context.SaveChanges();
		context.ChangeTracker.Clear();

		context.Database.Migrate();

		Job savedJob = context.Jobs.Single();

		Assert.That(savedJob.Id, Is.EqualTo(existingJob.Id));
		Assert.That(savedJob.Status, Is.EqualTo(existingJob.Status));
		Assert.That(savedJob.Location, Is.EqualTo(existingJob.Location));
		Assert.That(savedJob.Description, Is.EqualTo(existingJob.Description));
		Assert.That(
			savedJob.Requirements, Is.EqualTo(existingJob.Requirements));
		Assert.That(
			savedJob.SalaryCurrency, Is.EqualTo(existingJob.SalaryCurrency));
		Assert.That(savedJob.JobType, Is.EqualTo(existingJob.JobType));
		Assert.That(savedJob.RemoteType, Is.EqualTo(existingJob.RemoteType));
		Assert.That(savedJob.Notes, Is.EqualTo(existingJob.Notes));

		migrator.Migrate("20261116204352_UseCamelCaseColumns");
		context.ChangeTracker.Clear();
		Job restoredJob = context.Jobs.Single();

		Assert.That(restoredJob.Id, Is.EqualTo(existingJob.Id));
		Assert.That(restoredJob.Notes, Is.EqualTo(existingJob.Notes));
		Assert.That(
			restoredJob.Description, Is.EqualTo(existingJob.Description));
	}

	[Test]
	public void OptionalDetailsRollback_RejectsMissingValues()
	{
		using JobsDbContext context = CreateContext();
		context.Database.Migrate();

		Job job = CopyJob(TestJob!);
		job.Location = null;
		context.Jobs.Add(job);
		context.SaveChanges();

		IMigrator migrator = context.GetService<IMigrator>();
		SqliteException? databaseException = Assert.Throws<SqliteException>(
			() => migrator.Migrate("20261116204352_UseCamelCaseColumns"));

		Assert.That(
			databaseException!.SqliteExtendedErrorCode,
			Is.EqualTo(1299)); // SQLITE_CONSTRAINT_NOTNULL

		context.ChangeTracker.Clear();
		Job retainedJob = context.Jobs.Single();

		Assert.That(retainedJob.Location, Is.Null);
		Assert.That(
			context.Database.GetAppliedMigrations(),
			Does.Contain("20261116222114_AllowOptionalJobDetails"));
	}

	[TestCase(false, ApplicationStatus.Applied)]
	[TestCase(true, ApplicationStatus.Accepted)]
	public async Task Schema_JobRepositoryPreservesTypedValues(
		bool isArchived, ApplicationStatus applicationStatus)
	{
		using JobsDbContext context = CreateContext();
		LoadSchema(context);
		IJobRepository repository = new JobRepository(context);
		DateTime datePosted = new DateTime(2026, 11, 1, 9, 30, 0);
		DateTime dateApplied = new DateTime(2026, 11, 2, 14, 15, 0);
		Job job = new Job
		{
			Title = "SQL-created database",
			Company = "Test Company",
			Source = "SchemaTest",
			SourceJobId = "typed-values",
			SourceUrl = "https://example.com/jobs/typed-values",
			DatePosted = datePosted,
			DateApplied = dateApplied,
			SalaryMin = 123456.78m,
			SalaryMax = 234567.89m,
			Status = applicationStatus,
			IsArchived = isArchived,
			Notes = "Preserve application notes"
		};

		Job addedJob = await repository.AddAsync(job).ConfigureAwait(false);
		int jobIdentifier = addedJob.Id;
		DateTime dateScraped = addedJob.DateScraped;
		context.ChangeTracker.Clear();

		Job savedJob = await repository.GetByIdAsync(jobIdentifier)
			.ConfigureAwait(false);

		Assert.That(savedJob, Is.Not.Null);
		Assert.That(savedJob.Id, Is.GreaterThan(0));
		Assert.That(savedJob.Status, Is.EqualTo(applicationStatus));
		Assert.That(savedJob.IsArchived, Is.EqualTo(isArchived));
		Assert.That(savedJob.DatePosted, Is.EqualTo(datePosted));
		Assert.That(savedJob.DateApplied, Is.EqualTo(dateApplied));
		Assert.That(savedJob.DateScraped, Is.EqualTo(dateScraped));
		Assert.That(savedJob.SalaryMin, Is.EqualTo(123456.78m));
		Assert.That(savedJob.SalaryMax, Is.EqualTo(234567.89m));
		Assert.That(savedJob.Location, Is.Null);
		Assert.That(savedJob.Description, Is.Null);
		Assert.That(savedJob.Notes, Is.EqualTo("Preserve application notes"));

		savedJob.Notes = "Updated through the repository";
		savedJob.Status = ApplicationStatus.Interviewing;
		await repository.UpdateAsync(savedJob).ConfigureAwait(false);
		context.ChangeTracker.Clear();
		Job updatedJob = await repository.GetByIdAsync(jobIdentifier)
			.ConfigureAwait(false);

		Assert.That(
			updatedJob.Status, Is.EqualTo(ApplicationStatus.Interviewing));
		Assert.That(updatedJob.Notes, Is.EqualTo(savedJob.Notes));
		Assert.That(updatedJob.SalaryMin, Is.EqualTo(savedJob.SalaryMin));
		Assert.That(updatedJob.IsArchived, Is.EqualTo(isArchived));
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task Schema_StorageMapperWorksWithExistingRepository(
		bool includeOptionalValues)
	{
		using JobsDbContext context = CreateContext();
		LoadSchema(context);
		IJobRepository repository = new JobRepository(context);
		Job repositoryJob = CopyJob(TestJob!);
		repositoryJob.SalaryMin = 123456.789m;
		repositoryJob.SalaryMax = 234567.891m;
		repositoryJob.DateApplied = new DateTime(2026, 11, 2, 14, 15, 0);
		repositoryJob.Status = ApplicationStatus.Interviewing;
		repositoryJob.IsArchived = true;
		await repository.AddAsync(repositoryJob).ConfigureAwait(false);

		DbConnection connection = context.Database.GetDbConnection();
		DbContextOptionsBuilder<StorageContext> optionsBuilder =
			new DbContextOptionsBuilder<StorageContext>();
		optionsBuilder.UseSqlite(connection);
		using StorageContext storageContext =
			new StorageContext(optionsBuilder.Options);
		StorageJob storedRepositoryJob = await storageContext.Jobs.SingleAsync()
			.ConfigureAwait(false);
		Job mappedJob = JobStorageMapper.ToDomain(storedRepositoryJob);

		Assert.That(
			mappedJob, Is.EqualTo(repositoryJob).UsingPropertiesComparer());

		Job storageJob = CopyJob(TestJob!);
		storageJob.SourceJobId = "storage-job";
		storageJob.DateScraped = new DateTime(2026, 11, 3, 10, 20, 30);
		storageJob.DateScraped = storageJob.DateScraped.AddTicks(1234567);
		storageJob.IsArchived = includeOptionalValues;
		storageJob.Status = ApplicationStatus.Applied;

		if (includeOptionalValues)
		{
			storageJob.SalaryMin = 123456.789m;
			storageJob.SalaryMax = 234567.891m;
			storageJob.DateApplied = new DateTime(2026, 11, 4, 9, 15, 0);
			storageJob.Priority = 3;
		}
		else
		{
			storageJob.Location = null;
			storageJob.Description = null;
			storageJob.Requirements = null;
			storageJob.SalaryCurrency = null;
			storageJob.JobType = null;
			storageJob.RemoteType = null;
			storageJob.Notes = null;
		}

		StorageJob storedJob = JobStorageMapper.ToStorage(storageJob);
		storageContext.Jobs.Add(storedJob);
		await storageContext.SaveChangesAsync().ConfigureAwait(false);
		storageJob.Id = storedJob.Id;
		context.ChangeTracker.Clear();
		Job savedJob = await repository.GetByIdAsync(storedJob.Id)
			.ConfigureAwait(false);

		Assert.That(savedJob, Is.EqualTo(storageJob).UsingPropertiesComparer());

		savedJob.Notes = "Updated by existing repository";
		savedJob.Status = ApplicationStatus.Accepted;
		await repository.UpdateAsync(savedJob).ConfigureAwait(false);
		StorageJob updatedStorageJob = await storageContext.Jobs.AsNoTracking()
			.SingleAsync(job => job.Id == savedJob.Id).ConfigureAwait(false);
		Job updatedJob = JobStorageMapper.ToDomain(updatedStorageJob);

		Assert.That(updatedJob, Is.EqualTo(savedJob).UsingPropertiesComparer());
	}

	[Test]
	public void SqlUpgrades_MatchMasterSchema()
	{
		using JobsDbContext upgradedContext = CreateContext();
		ApplySqlUpgrades(upgradedContext);
		using JobsDbContext expectedContext = CreateContext();
		LoadSchema(expectedContext);

		List<string> expectedSchema = ReadSchema(expectedContext);
		List<string> actualSchema = ReadSchema(upgradedContext);

		Assert.That(actualSchema, Is.EqualTo(expectedSchema));
		Assert.That(
			upgradedContext.Database.GetAppliedMigrations(),
			Is.EqualTo(expectedContext.Database.GetAppliedMigrations()));
		Assert.That(upgradedContext.Database.GetPendingMigrations(), Is.Empty);
	}

	[TestCase("20260723233159_InitialCreate")]
	[TestCase("20261116204352_UseCamelCaseColumns")]
	public void SqlUpgrades_PreserveExistingEfDatabase(string previousVersion)
	{
		using JobsDbContext context = CreateContext();
		IMigrator migrator = context.GetService<IMigrator>();
		migrator.Migrate(previousVersion);
		Job existingJob = CopyJob(TestJob!);
		existingJob.Id = 42;
		existingJob.Status = ApplicationStatus.Applied;
		existingJob.Notes = "Keep application history";
		existingJob.SalaryMin = 123456.789m;
		context.Jobs.Add(existingJob);
		context.SaveChanges();
		context.ChangeTracker.Clear();

		ApplySqlUpgrades(context);
		Job savedJob = context.Jobs.Single();

		Assert.That(
			savedJob, Is.EqualTo(existingJob).UsingPropertiesComparer());
		Assert.That(context.Database.GetPendingMigrations(), Is.Empty);
	}

	[TestCase(false)]
	[TestCase(true)]
	public void SqlUpgrades_PreserveConsumedIdentifiers(bool retainJob)
	{
		using JobsDbContext context = CreateContext();
		ApplySqlUpgrades(context, 2);
		Job deletedJob = CopyJob(TestJob!);
		deletedJob.Id = 500;
		context.Jobs.Add(deletedJob);
		context.SaveChanges();
		context.Jobs.Remove(deletedJob);
		context.SaveChanges();

		if (retainJob)
		{
			Job retainedJob = CopyJob(TestJob!);
			retainedJob.Id = 42;
			context.Jobs.Add(retainedJob);
			context.SaveChanges();
		}

		ApplySqlUpgrades(context);
		Job newJob = CopyJob(TestJob!);
		newJob.SourceJobId = "after-sql-upgrade";
		context.Jobs.Add(newJob);
		context.SaveChanges();

		Assert.That(newJob.Id, Is.GreaterThan(500));
	}

	[Test]
	public void SqlUpgrades_CurrentDatabasePreservesData()
	{
		using JobsDbContext context = CreateContext();
		LoadSchema(context);
		Job existingJob = CopyJob(TestJob!);
		existingJob.Status = ApplicationStatus.Applied;
		existingJob.Notes = "Preserve current database";
		context.Jobs.Add(existingJob);
		context.SaveChanges();
		context.ChangeTracker.Clear();
		List<string> expectedSchema = ReadSchema(context);

		ApplySqlUpgrades(context);
		Job savedJob = context.Jobs.Single();

		Assert.That(
			savedJob, Is.EqualTo(existingJob).UsingPropertiesComparer());
		Assert.That(ReadSchema(context), Is.EqualTo(expectedSchema));
	}

	[Test]
	public void SqlUpgrade_FailureRollsBackSchemaDataAndHistory()
	{
		using JobsDbContext context = CreateContext();
		ApplySqlUpgrades(context, 2);
		Job existingJob = CopyJob(TestJob!);
		existingJob.Status = ApplicationStatus.Applied;
		existingJob.Notes = "Preserve on failure";
		context.Jobs.Add(existingJob);
		context.SaveChanges();
		context.ChangeTracker.Clear();
		List<string> expectedSchema = ReadSchema(context);
		string script = ReadUpgradeScript("allowOptionalJobDetails.sql");
		string failingStatement =
			"INSERT INTO MissingUpgradeTable DEFAULT VALUES;\n" +
			"CREATE INDEX \"IX_Jobs_company\"";
		script = script.Replace(
			"CREATE INDEX \"IX_Jobs_company\"",
			failingStatement,
			StringComparison.Ordinal);

		Assert.Throws<SqliteException>(() => ExecuteSql(context, script));
		ExecuteSql(context, "ROLLBACK; PRAGMA foreign_keys = ON;");
		Job savedJob = context.Jobs.Single();

		Assert.That(
			savedJob, Is.EqualTo(existingJob).UsingPropertiesComparer());
		Assert.That(ReadSchema(context), Is.EqualTo(expectedSchema));
		Assert.That(
			context.Database.GetAppliedMigrations(),
			Does.Not.Contain("20261116222114_AllowOptionalJobDetails"));
	}

	private static void ApplySqlUpgrades(
		JobsDbContext context, int maximumUpgrades = int.MaxValue)
	{
		string orderPath = Path.Combine(
			TestContext.CurrentContext.TestDirectory,
			"Database",
			"Upgrades",
			"upgradeOrder.txt");
		string[] upgrades = File.ReadAllLines(orderPath);
		HashSet<string> appliedVersions = context.Database
			.GetAppliedMigrations().ToHashSet(StringComparer.Ordinal);

		foreach (string upgrade in upgrades.Take(maximumUpgrades))
		{
			string[] fields = upgrade.Split(
				' ', StringSplitOptions.RemoveEmptyEntries);
			string version = fields[0];
			string fileName = fields[1];

			if (appliedVersions.Contains(version))
			{
				continue;
			}

			string script = ReadUpgradeScript(fileName);
			ExecuteSql(context, script);
		}
	}

	private static string ReadUpgradeScript(string fileName)
	{
		string path = Path.Combine(
			TestContext.CurrentContext.TestDirectory,
			"Database",
			"Upgrades",
			fileName);
		return File.ReadAllText(path);
	}

	private static JobsDbContext CreateContext()
	{
		DbContextOptionsBuilder<JobsDbContext> optionsBuilder =
			new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite("Data Source=:memory:");

		JobsDbContext context = new JobsDbContext(optionsBuilder.Options);
		context.Database.OpenConnection();
		return context;
	}

	private static void LoadSchema(JobsDbContext context)
	{
		string schemaPath = Path.Combine(
			TestContext.CurrentContext.TestDirectory, "Database", "schema.sql");
		string schema = File.ReadAllText(schemaPath);
		ExecuteSql(context, schema);
	}

	[SuppressMessage(
		"Security",
		"CA2100",
		Justification = "SQL comes only from checked-in database fixtures.")]
	private static void ExecuteSql(JobsDbContext context, string script)
	{
		DbConnection connection = context.Database.GetDbConnection();
		using DbCommand command = connection.CreateCommand();
		command.CommandText = script;
		command.ExecuteNonQuery();
	}

	private static List<string> ReadSchema(JobsDbContext context)
	{
		DbConnection connection = context.Database.GetDbConnection();
		using DbCommand command = connection.CreateCommand();
		command.CommandText =
			"SELECT type, name, sql FROM sqlite_schema " +
			"WHERE name NOT LIKE 'sqlite_%' " +
			"AND name <> '__EFMigrationsLock' AND sql IS NOT NULL " +
			"ORDER BY type, name";

		using DbDataReader reader = command.ExecuteReader();
		List<string> definitions = new List<string>();

		while (reader.Read())
		{
			string objectType = reader.GetString(0);
			string objectName = reader.GetString(1);
			string definition = reader.GetString(2);
			string normalizedDefinition =
				Regex.Replace(definition, @"\s+", " ");
			normalizedDefinition = normalizedDefinition.Trim();
			string schemaEntry =
				objectType + " " + objectName + " " + normalizedDefinition;
			definitions.Add(schemaEntry);
		}

		return definitions;
	}
}
