/////////////////////////////////////////////////////////////////////////////
// <copyright file="InitialCreate.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalZenWorks.JobsDb.Library.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "Credentials",
			columns: table => new
			{
				Id = table.Column<int>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				Username = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
				EncryptedPassword = table.Column<string>(type: "TEXT", nullable: false),
				CookieData = table.Column<string>(type: "TEXT", nullable: false),
				LastUsed = table.Column<DateTime>(type: "TEXT", nullable: true),
				IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Credentials", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "Jobs",
			columns: table => new
			{
				Id = table.Column<int>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
				Company = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
				Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
				Description = table.Column<string>(type: "TEXT", nullable: false),
				Requirements = table.Column<string>(type: "TEXT", nullable: false),
				Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
				SourceJobId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
				SalaryMin = table.Column<decimal>(type: "TEXT", nullable: true),
				SalaryMax = table.Column<decimal>(type: "TEXT", nullable: true),
				SalaryCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
				JobType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				RemoteType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				DatePosted = table.Column<DateTime>(type: "TEXT", nullable: false),
				DateScraped = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
				DateApplied = table.Column<DateTime>(type: "TEXT", nullable: true),
				Status = table.Column<string>(type: "TEXT", nullable: false),
				Priority = table.Column<int>(type: "INTEGER", nullable: true),
				Notes = table.Column<string>(type: "TEXT", nullable: false),
				IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Jobs", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "ScraperLogs",
			columns: table => new
			{
				Id = table.Column<int>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
				JobsFound = table.Column<int>(type: "INTEGER", nullable: false),
				JobsAdded = table.Column<int>(type: "INTEGER", nullable: false),
				JobsUpdated = table.Column<int>(type: "INTEGER", nullable: false),
				Success = table.Column<bool>(type: "INTEGER", nullable: false),
				ErrorMessage = table.Column<string>(type: "TEXT", nullable: false),
				DurationMs = table.Column<int>(type: "INTEGER", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ScraperLogs", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "SearchFilters",
			columns: table => new
			{
				Id = table.Column<int>(type: "INTEGER", nullable: false)
					.Annotation("Sqlite:Autoincrement", true),
				Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
				Keywords = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
				Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
				Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
				IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
				CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
				LastRunDate = table.Column<DateTime>(type: "TEXT", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_SearchFilters", x => x.Id);
			});

		migrationBuilder.CreateIndex(
			name: "IX_Credentials_Source",
			table: "Credentials",
			column: "Source",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_Company",
			table: "Jobs",
			column: "Company");

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_DatePosted",
			table: "Jobs",
			column: "DatePosted");

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_IsArchived",
			table: "Jobs",
			column: "IsArchived");

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_Location",
			table: "Jobs",
			column: "Location");

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_Source_SourceJobId",
			table: "Jobs",
			columns: new[] { "Source", "SourceJobId" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_SourceJobId",
			table: "Jobs",
			column: "SourceJobId");

		migrationBuilder.CreateIndex(
			name: "IX_Jobs_Status",
			table: "Jobs",
			column: "Status");

		migrationBuilder.CreateIndex(
			name: "IX_ScraperLogs_Source",
			table: "ScraperLogs",
			column: "Source");

		migrationBuilder.CreateIndex(
			name: "IX_ScraperLogs_Success",
			table: "ScraperLogs",
			column: "Success");

		migrationBuilder.CreateIndex(
			name: "IX_ScraperLogs_Timestamp",
			table: "ScraperLogs",
			column: "Timestamp");

		migrationBuilder.CreateIndex(
			name: "IX_SearchFilters_IsActive",
			table: "SearchFilters",
			column: "IsActive");

		migrationBuilder.CreateIndex(
			name: "IX_SearchFilters_Name",
			table: "SearchFilters",
			column: "Name");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "Credentials");

		migrationBuilder.DropTable(
			name: "Jobs");

		migrationBuilder.DropTable(
			name: "ScraperLogs");

		migrationBuilder.DropTable(
			name: "SearchFilters");
	}
}
