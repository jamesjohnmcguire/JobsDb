/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDbSchemaMetadata.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SQLitePCL;

/// <summary>
/// Compares SQLite metadata for the schema features JobsDb uses.
/// </summary>
internal static class JobsDbSchemaMetadata
{
	private const string ApplicationTables = """
		SELECT name, type, ncol, wr, strict
		FROM pragma_table_list
		WHERE schema = 'main' AND name NOT GLOB 'sqlite_*'
			AND name <> '__EFMigrationsLock'
		""";

	private const string Tables = ApplicationTables + "\nORDER BY name";

	private const string Columns = """
		SELECT tables.name, column.name, column.type,
			column.dflt_value, column.pk, column.hidden
		FROM applicationTables AS tables
		JOIN pragma_table_xinfo(tables.name, 'main') AS column
		WHERE tables.type = 'table'
		ORDER BY tables.name, column.cid
		""";

	private const string Indexes = """
		SELECT tables.name, indexes.name, indexes."unique", indexes.origin,
			indexes.partial, columns.cid, columns.name, columns."desc",
			columns.coll, columns."key"
		FROM applicationTables AS tables
		JOIN pragma_index_list(tables.name, 'main') AS indexes
		JOIN pragma_index_xinfo(indexes.name, 'main') AS columns
		ORDER BY tables.name, indexes.name, columns.seqno
		""";

	private const string ForeignKeys = """
		SELECT tables.name, keys.id, keys.seq, keys."table", keys."from",
			keys."to", keys.on_update, keys.on_delete, keys.match
		FROM applicationTables AS tables
		JOIN pragma_foreign_key_list(tables.name, 'main') AS keys
		ORDER BY tables.name, keys.id, keys.seq
		""";

	/// <summary>
	/// Checks tables, columns, indexes, and foreign keys on open connections.
	/// </summary>
	/// <param name="actual">The upgraded database.</param>
	/// <param name="expected">The fresh database.</param>
	public static void AssertMatches(
		SqliteConnection actual, SqliteConnection expected)
	{
		Assert.That(
			ReadRows(actual, Tables),
			Is.EqualTo(ReadRows(expected, Tables)),
			"SQLite tables");
		Assert.That(
			ReadColumns(actual),
			Is.EqualTo(ReadColumns(expected)),
			"SQLite columns");
		Assert.That(
			ReadRows(actual, ForApplicationTables(Indexes)),
			Is.EqualTo(ReadRows(expected, ForApplicationTables(Indexes))),
			"SQLite indexes");
		Assert.That(
			ReadRows(actual, ForApplicationTables(ForeignKeys)),
			Is.EqualTo(ReadRows(expected, ForApplicationTables(ForeignKeys))),
			"SQLite foreign keys");
	}

	/// <summary>
	/// Reads column attributes, including collation and automatic key policy.
	/// </summary>
	/// <param name="connection">The open database connection.</param>
	/// <returns>Column metadata in table and column order.</returns>
	public static List<object[]> ReadColumns(SqliteConnection connection)
	{
		List<object[]> columns = ReadRows(
			connection, ForApplicationTables(Columns));
		List<object[]> descriptions = new List<object[]>();
		foreach (object[] column in columns)
		{
			descriptions.Add(ReadColumnAttributes(connection, column));
		}

		return descriptions;
	}

	private static string ForApplicationTables(string query)
	{
		return "WITH applicationTables AS (" + ApplicationTables + ")\n" +
			query;
	}

	private static object[] ReadColumnAttributes(
		SqliteConnection connection, object[] column)
	{
		string tableName = (string)column[0];
		string columnName = (string)column[1];
		int result = raw.sqlite3_table_column_metadata(
			connection.Handle,
			"main",
			tableName,
			columnName,
			out _,
			out string collation,
			out int notNull,
			out _,
			out int autoIncrement);
		if (result != raw.SQLITE_OK)
		{
			throw new InvalidOperationException(
				$"Cannot read SQLite metadata for {tableName}.{columnName}.");
		}

		// SQLite generates a key for NULL in an AUTOINCREMENT column.
		// An explicit NOT NULL declaration does not change that behavior.
		if (autoIncrement != 0)
		{
			notNull = 1;
		}

		return
		[
			tableName, columnName, column[2], notNull, column[3],
			column[4], column[5], collation, autoIncrement
		];
	}

	[SuppressMessage(
		"Security",
		"CA2100",
		Justification = "The queries use only fixed SQLite metadata SQL.")]
	private static List<object[]> ReadRows(
		SqliteConnection connection, string query)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = query;
		using SqliteDataReader reader = command.ExecuteReader();
		List<object[]> rows = new List<object[]>();

		while (reader.Read())
		{
			object[] fields = new object[reader.FieldCount];
			reader.GetValues(fields);
			rows.Add(fields);
		}

		return rows;
	}
}
