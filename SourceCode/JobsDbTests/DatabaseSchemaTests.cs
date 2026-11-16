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
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

[TestFixture]
internal sealed class DatabaseSchemaTests : BaseTestsSupport
{
	[Test]
	public void Schema_MatchesMigratedDatabase()
	{
		using JobsDbContext migratedContext = CreateContext();
		migratedContext.Initialize();

		using JobsDbContext schemaContext = CreateContext();
		LoadSchema(schemaContext);

		List<string> expectedSchema = ReadSchema(migratedContext);
		List<string> actualSchema = ReadSchema(schemaContext);

		Assert.That(actualSchema, Is.EqualTo(expectedSchema));
		Assert.That(
			schemaContext.Database.GetAppliedMigrations(),
			Is.EqualTo(migratedContext.Database.GetAppliedMigrations()));
	}

	[Test]
	public void Schema_InitializePreservesExistingData()
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

		schemaContext.Initialize();
		schemaContext.Initialize();

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

		context.Initialize();

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

	private static JobsDbContext CreateContext()
	{
		DbContextOptionsBuilder<JobsDbContext> optionsBuilder =
			new DbContextOptionsBuilder<JobsDbContext>();
		optionsBuilder.UseSqlite("Data Source=:memory:");

		JobsDbContext context = new JobsDbContext(optionsBuilder.Options);
		context.Database.OpenConnection();
		return context;
	}

	[SuppressMessage(
		"Security",
		"CA2100",
		Justification = "SQL comes only from the checked-in schema fixture.")]
	private static void LoadSchema(JobsDbContext context)
	{
		string schemaPath = Path.Combine(
			TestContext.CurrentContext.TestDirectory, "Database", "schema.sql");
		string schema = File.ReadAllText(schemaPath);
		DbConnection connection = context.Database.GetDbConnection();
		using DbCommand command = connection.CreateCommand();
		command.CommandText = schema;
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
