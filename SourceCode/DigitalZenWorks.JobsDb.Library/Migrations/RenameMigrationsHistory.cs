/////////////////////////////////////////////////////////////////////////////
// <copyright file="RenameMigrationsHistory.cs"
// company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Records the version at which the history table name changed.
/// </summary>
[DbContext(typeof(JobsDbContext))]
[Migration("20261123220231_RenameMigrationsHistory")]
public sealed class RenameMigrationsHistory : Migration
{
	/// <inheritdoc/>
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// EF creates the configured history table on a fresh database.
		// The native SQL updater renames the table on existing databases.
	}

	/// <inheritdoc/>
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// EF continues using the configured history table after rollback.
	}
}
