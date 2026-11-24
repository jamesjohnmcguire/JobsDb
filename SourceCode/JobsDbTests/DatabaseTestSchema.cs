/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseTestSchema.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

/// <summary>
/// Prepares test databases from the checked-in master schema.
/// </summary>
internal static class DatabaseTestSchema
{
	/// <summary>
	/// Creates a test database using the master schema.
	/// </summary>
	/// <param name="databasePath">
	/// The SQLite database filename to create.
	/// </param>
	public static void Create(string databasePath)
	{
		SqliteConnectionStringBuilder settings =
			new SqliteConnectionStringBuilder();
		settings.DataSource = databasePath;
		settings.Pooling = false;
		using SqliteConnection connection =
			new SqliteConnection(settings.ConnectionString);
		connection.Open();
		Load(connection);
	}

	/// <summary>
	/// Loads the master schema on a database connection.
	/// </summary>
	/// <param name="connection">
	/// The connection on which to execute the schema.
	/// </param>
	[SuppressMessage(
		"Security",
		"CA2100",
		Justification = "SQL comes only from the checked-in schema fixture.")]
	public static void Load(DbConnection connection)
	{
		string path = Path.Combine(
			TestContext.CurrentContext.TestDirectory, "Database", "schema.sql");
		string script = File.ReadAllText(path);
		using DbCommand command = connection.CreateCommand();
		command.CommandText = script;
		command.ExecuteNonQuery();
	}
}
