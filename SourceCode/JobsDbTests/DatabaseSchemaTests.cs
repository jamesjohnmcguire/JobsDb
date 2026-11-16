/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseSchemaTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
internal sealed class DatabaseSchemaTests
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
