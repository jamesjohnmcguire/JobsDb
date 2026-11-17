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

internal static class DatabaseTestSchema
{
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
