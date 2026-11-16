/////////////////////////////////////////////////////////////////////////////
// <copyright file="20261116204352_UseCamelCaseColumns.cs"
// company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable disable

namespace DigitalZenWorks.JobsDb.Library.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class UseCamelCaseColumns : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		ArgumentNullException.ThrowIfNull(migrationBuilder);

		migrationBuilder.RenameColumn(
			name: "Source",
			table: "SearchFilters",
			newName: "source");

		migrationBuilder.RenameColumn(
			name: "Name",
			table: "SearchFilters",
			newName: "name");

		migrationBuilder.RenameColumn(
			name: "Location",
			table: "SearchFilters",
			newName: "location");

		migrationBuilder.RenameColumn(
			name: "LastRunDate",
			table: "SearchFilters",
			newName: "lastRunDate");

		migrationBuilder.RenameColumn(
			name: "Keywords",
			table: "SearchFilters",
			newName: "keywords");

		migrationBuilder.RenameColumn(
			name: "IsActive",
			table: "SearchFilters",
			newName: "isActive");

		migrationBuilder.RenameColumn(
			name: "CreatedDate",
			table: "SearchFilters",
			newName: "createdDate");

		migrationBuilder.RenameColumn(
			name: "Id",
			table: "SearchFilters",
			newName: "id");

		migrationBuilder.RenameIndex(
			name: "IX_SearchFilters_Name",
			table: "SearchFilters",
			newName: "IX_SearchFilters_name");

		migrationBuilder.RenameIndex(
			name: "IX_SearchFilters_IsActive",
			table: "SearchFilters",
			newName: "IX_SearchFilters_isActive");

		migrationBuilder.RenameColumn(
			name: "Timestamp",
			table: "ScraperLogs",
			newName: "timestamp");

		migrationBuilder.RenameColumn(
			name: "Success",
			table: "ScraperLogs",
			newName: "success");

		migrationBuilder.RenameColumn(
			name: "Source",
			table: "ScraperLogs",
			newName: "source");

		migrationBuilder.RenameColumn(
			name: "JobsUpdated",
			table: "ScraperLogs",
			newName: "jobsUpdated");

		migrationBuilder.RenameColumn(
			name: "JobsFound",
			table: "ScraperLogs",
			newName: "jobsFound");

		migrationBuilder.RenameColumn(
			name: "JobsAdded",
			table: "ScraperLogs",
			newName: "jobsAdded");

		migrationBuilder.RenameColumn(
			name: "ErrorMessage",
			table: "ScraperLogs",
			newName: "errorMessage");

		migrationBuilder.RenameColumn(
			name: "DurationMs",
			table: "ScraperLogs",
			newName: "durationMs");

		migrationBuilder.RenameColumn(
			name: "Id",
			table: "ScraperLogs",
			newName: "id");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_Timestamp",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_timestamp");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_Success",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_success");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_Source",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_source");

		migrationBuilder.RenameColumn(
			name: "Title",
			table: "Jobs",
			newName: "title");

		migrationBuilder.RenameColumn(
			name: "Status",
			table: "Jobs",
			newName: "status");

		migrationBuilder.RenameColumn(
			name: "SourceUrl",
			table: "Jobs",
			newName: "sourceUrl");

		migrationBuilder.RenameColumn(
			name: "SourceJobId",
			table: "Jobs",
			newName: "sourceJobId");

		migrationBuilder.RenameColumn(
			name: "Source",
			table: "Jobs",
			newName: "source");

		migrationBuilder.RenameColumn(
			name: "SalaryMin",
			table: "Jobs",
			newName: "salaryMin");

		migrationBuilder.RenameColumn(
			name: "SalaryMax",
			table: "Jobs",
			newName: "salaryMax");

		migrationBuilder.RenameColumn(
			name: "SalaryCurrency",
			table: "Jobs",
			newName: "salaryCurrency");

		migrationBuilder.RenameColumn(
			name: "Requirements",
			table: "Jobs",
			newName: "requirements");

		migrationBuilder.RenameColumn(
			name: "RemoteType",
			table: "Jobs",
			newName: "remoteType");

		migrationBuilder.RenameColumn(
			name: "Priority",
			table: "Jobs",
			newName: "priority");

		migrationBuilder.RenameColumn(
			name: "Notes",
			table: "Jobs",
			newName: "notes");

		migrationBuilder.RenameColumn(
			name: "Location",
			table: "Jobs",
			newName: "location");

		migrationBuilder.RenameColumn(
			name: "JobType",
			table: "Jobs",
			newName: "jobType");

		migrationBuilder.RenameColumn(
			name: "IsArchived",
			table: "Jobs",
			newName: "isArchived");

		migrationBuilder.RenameColumn(
			name: "Description",
			table: "Jobs",
			newName: "description");

		migrationBuilder.RenameColumn(
			name: "DateScraped",
			table: "Jobs",
			newName: "dateScraped");

		migrationBuilder.RenameColumn(
			name: "DatePosted",
			table: "Jobs",
			newName: "datePosted");

		migrationBuilder.RenameColumn(
			name: "DateApplied",
			table: "Jobs",
			newName: "dateApplied");

		migrationBuilder.RenameColumn(
			name: "Company",
			table: "Jobs",
			newName: "company");

		migrationBuilder.RenameColumn(
			name: "Id",
			table: "Jobs",
			newName: "id");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_Status",
			table: "Jobs",
			newName: "IX_Jobs_status");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_SourceJobId",
			table: "Jobs",
			newName: "IX_Jobs_sourceJobId");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_Source_SourceJobId",
			table: "Jobs",
			newName: "IX_Jobs_source_sourceJobId");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_Location",
			table: "Jobs",
			newName: "IX_Jobs_location");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_IsArchived",
			table: "Jobs",
			newName: "IX_Jobs_isArchived");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_DatePosted",
			table: "Jobs",
			newName: "IX_Jobs_datePosted");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_Company",
			table: "Jobs",
			newName: "IX_Jobs_company");

		migrationBuilder.RenameColumn(
			name: "Username",
			table: "Credentials",
			newName: "username");

		migrationBuilder.RenameColumn(
			name: "Source",
			table: "Credentials",
			newName: "source");

		migrationBuilder.RenameColumn(
			name: "LastUsed",
			table: "Credentials",
			newName: "lastUsed");

		migrationBuilder.RenameColumn(
			name: "IsActive",
			table: "Credentials",
			newName: "isActive");

		migrationBuilder.RenameColumn(
			name: "EncryptedPassword",
			table: "Credentials",
			newName: "encryptedPassword");

		migrationBuilder.RenameColumn(
			name: "CookieData",
			table: "Credentials",
			newName: "cookieData");

		migrationBuilder.RenameColumn(
			name: "Id",
			table: "Credentials",
			newName: "id");

		migrationBuilder.RenameIndex(
			name: "IX_Credentials_Source",
			table: "Credentials",
			newName: "IX_Credentials_source");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		ArgumentNullException.ThrowIfNull(migrationBuilder);

		migrationBuilder.RenameColumn(
			name: "source",
			table: "SearchFilters",
			newName: "Source");

		migrationBuilder.RenameColumn(
			name: "name",
			table: "SearchFilters",
			newName: "Name");

		migrationBuilder.RenameColumn(
			name: "location",
			table: "SearchFilters",
			newName: "Location");

		migrationBuilder.RenameColumn(
			name: "lastRunDate",
			table: "SearchFilters",
			newName: "LastRunDate");

		migrationBuilder.RenameColumn(
			name: "keywords",
			table: "SearchFilters",
			newName: "Keywords");

		migrationBuilder.RenameColumn(
			name: "isActive",
			table: "SearchFilters",
			newName: "IsActive");

		migrationBuilder.RenameColumn(
			name: "createdDate",
			table: "SearchFilters",
			newName: "CreatedDate");

		migrationBuilder.RenameColumn(
			name: "id",
			table: "SearchFilters",
			newName: "Id");

		migrationBuilder.RenameIndex(
			name: "IX_SearchFilters_name",
			table: "SearchFilters",
			newName: "IX_SearchFilters_Name");

		migrationBuilder.RenameIndex(
			name: "IX_SearchFilters_isActive",
			table: "SearchFilters",
			newName: "IX_SearchFilters_IsActive");

		migrationBuilder.RenameColumn(
			name: "timestamp",
			table: "ScraperLogs",
			newName: "Timestamp");

		migrationBuilder.RenameColumn(
			name: "success",
			table: "ScraperLogs",
			newName: "Success");

		migrationBuilder.RenameColumn(
			name: "source",
			table: "ScraperLogs",
			newName: "Source");

		migrationBuilder.RenameColumn(
			name: "jobsUpdated",
			table: "ScraperLogs",
			newName: "JobsUpdated");

		migrationBuilder.RenameColumn(
			name: "jobsFound",
			table: "ScraperLogs",
			newName: "JobsFound");

		migrationBuilder.RenameColumn(
			name: "jobsAdded",
			table: "ScraperLogs",
			newName: "JobsAdded");

		migrationBuilder.RenameColumn(
			name: "errorMessage",
			table: "ScraperLogs",
			newName: "ErrorMessage");

		migrationBuilder.RenameColumn(
			name: "durationMs",
			table: "ScraperLogs",
			newName: "DurationMs");

		migrationBuilder.RenameColumn(
			name: "id",
			table: "ScraperLogs",
			newName: "Id");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_timestamp",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_Timestamp");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_success",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_Success");

		migrationBuilder.RenameIndex(
			name: "IX_ScraperLogs_source",
			table: "ScraperLogs",
			newName: "IX_ScraperLogs_Source");

		migrationBuilder.RenameColumn(
			name: "title",
			table: "Jobs",
			newName: "Title");

		migrationBuilder.RenameColumn(
			name: "status",
			table: "Jobs",
			newName: "Status");

		migrationBuilder.RenameColumn(
			name: "sourceUrl",
			table: "Jobs",
			newName: "SourceUrl");

		migrationBuilder.RenameColumn(
			name: "sourceJobId",
			table: "Jobs",
			newName: "SourceJobId");

		migrationBuilder.RenameColumn(
			name: "source",
			table: "Jobs",
			newName: "Source");

		migrationBuilder.RenameColumn(
			name: "salaryMin",
			table: "Jobs",
			newName: "SalaryMin");

		migrationBuilder.RenameColumn(
			name: "salaryMax",
			table: "Jobs",
			newName: "SalaryMax");

		migrationBuilder.RenameColumn(
			name: "salaryCurrency",
			table: "Jobs",
			newName: "SalaryCurrency");

		migrationBuilder.RenameColumn(
			name: "requirements",
			table: "Jobs",
			newName: "Requirements");

		migrationBuilder.RenameColumn(
			name: "remoteType",
			table: "Jobs",
			newName: "RemoteType");

		migrationBuilder.RenameColumn(
			name: "priority",
			table: "Jobs",
			newName: "Priority");

		migrationBuilder.RenameColumn(
			name: "notes",
			table: "Jobs",
			newName: "Notes");

		migrationBuilder.RenameColumn(
			name: "location",
			table: "Jobs",
			newName: "Location");

		migrationBuilder.RenameColumn(
			name: "jobType",
			table: "Jobs",
			newName: "JobType");

		migrationBuilder.RenameColumn(
			name: "isArchived",
			table: "Jobs",
			newName: "IsArchived");

		migrationBuilder.RenameColumn(
			name: "description",
			table: "Jobs",
			newName: "Description");

		migrationBuilder.RenameColumn(
			name: "dateScraped",
			table: "Jobs",
			newName: "DateScraped");

		migrationBuilder.RenameColumn(
			name: "datePosted",
			table: "Jobs",
			newName: "DatePosted");

		migrationBuilder.RenameColumn(
			name: "dateApplied",
			table: "Jobs",
			newName: "DateApplied");

		migrationBuilder.RenameColumn(
			name: "company",
			table: "Jobs",
			newName: "Company");

		migrationBuilder.RenameColumn(
			name: "id",
			table: "Jobs",
			newName: "Id");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_status",
			table: "Jobs",
			newName: "IX_Jobs_Status");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_sourceJobId",
			table: "Jobs",
			newName: "IX_Jobs_SourceJobId");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_source_sourceJobId",
			table: "Jobs",
			newName: "IX_Jobs_Source_SourceJobId");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_location",
			table: "Jobs",
			newName: "IX_Jobs_Location");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_isArchived",
			table: "Jobs",
			newName: "IX_Jobs_IsArchived");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_datePosted",
			table: "Jobs",
			newName: "IX_Jobs_DatePosted");

		migrationBuilder.RenameIndex(
			name: "IX_Jobs_company",
			table: "Jobs",
			newName: "IX_Jobs_Company");

		migrationBuilder.RenameColumn(
			name: "username",
			table: "Credentials",
			newName: "Username");

		migrationBuilder.RenameColumn(
			name: "source",
			table: "Credentials",
			newName: "Source");

		migrationBuilder.RenameColumn(
			name: "lastUsed",
			table: "Credentials",
			newName: "LastUsed");

		migrationBuilder.RenameColumn(
			name: "isActive",
			table: "Credentials",
			newName: "IsActive");

		migrationBuilder.RenameColumn(
			name: "encryptedPassword",
			table: "Credentials",
			newName: "EncryptedPassword");

		migrationBuilder.RenameColumn(
			name: "cookieData",
			table: "Credentials",
			newName: "CookieData");

		migrationBuilder.RenameColumn(
			name: "id",
			table: "Credentials",
			newName: "Id");

		migrationBuilder.RenameIndex(
			name: "IX_Credentials_source",
			table: "Credentials",
			newName: "IX_Credentials_Source");
	}
}
