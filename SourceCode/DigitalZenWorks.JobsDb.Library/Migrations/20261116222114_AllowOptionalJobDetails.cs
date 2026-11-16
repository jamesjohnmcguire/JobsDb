/////////////////////////////////////////////////////////////////////////////
// <copyright file="20261116222114_AllowOptionalJobDetails.cs"
// company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable disable

namespace DigitalZenWorks.JobsDb.Library.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AllowOptionalJobDetails : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		ArgumentNullException.ThrowIfNull(migrationBuilder);

		migrationBuilder.AlterColumn<string>(
			name: "salaryCurrency",
			table: "Jobs",
			type: "TEXT",
			maxLength: 10,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 10);

		migrationBuilder.AlterColumn<string>(
			name: "requirements",
			table: "Jobs",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<string>(
			name: "remoteType",
			table: "Jobs",
			type: "TEXT",
			maxLength: 50,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 50);

		migrationBuilder.AlterColumn<string>(
			name: "notes",
			table: "Jobs",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<string>(
			name: "location",
			table: "Jobs",
			type: "TEXT",
			maxLength: 200,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 200);

		migrationBuilder.AlterColumn<string>(
			name: "jobType",
			table: "Jobs",
			type: "TEXT",
			maxLength: 50,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 50);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "Jobs",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		ArgumentNullException.ThrowIfNull(migrationBuilder);

		migrationBuilder.AlterColumn<string>(
			name: "salaryCurrency",
			table: "Jobs",
			type: "TEXT",
			maxLength: 10,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 10,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "requirements",
			table: "Jobs",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "remoteType",
			table: "Jobs",
			type: "TEXT",
			maxLength: 50,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 50,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "notes",
			table: "Jobs",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "location",
			table: "Jobs",
			type: "TEXT",
			maxLength: 200,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 200,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "jobType",
			table: "Jobs",
			type: "TEXT",
			maxLength: 50,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldMaxLength: 50,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "description",
			table: "Jobs",
			type: "TEXT",
			nullable: false,
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);
	}
}
