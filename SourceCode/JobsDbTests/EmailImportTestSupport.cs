/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportTestSupport.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System.IO;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using NUnit.Framework;

/// <summary>
/// Prepares a database and local message fixtures for import tests.
/// </summary>
internal abstract class EmailImportTestSupport : BaseTestsSupport
{
	/// <summary>
	/// Gets the temporary input directory for the current batch.
	/// </summary>
	protected string ImportDirectory { get; private set; } = string.Empty;

	/// <summary>
	/// Creates the test database and input directory.
	/// </summary>
	[SetUp]
	public void SetUpImport()
	{
		DatabaseTestSchema.Create(TestDbPath!);
		ImportDirectory = TestDbPath + ".email batch";
		Directory.CreateDirectory(ImportDirectory);
	}

	/// <summary>
	/// Removes the temporary database and message exports.
	/// </summary>
	[TearDown]
	public void TearDownImport()
	{
		using JobsDbContext cleanup = new JobsDbContext(TestDbPath!);
		cleanup.Database.EnsureDeleted();
		foreach (string path in Directory.GetFiles(ImportDirectory))
		{
			File.Delete(path);
		}

		Directory.Delete(ImportDirectory);
	}

	/// <summary>
	/// Builds a LinkedIn listing card for an HTML email fixture.
	/// </summary>
	/// <param name="identifier">
	/// The listing identifier embedded in the link.
	/// </param>
	/// <param name="title">The listing title shown in the card.</param>
	/// <param name="company">
	/// The company shown in the card, or null to omit it.
	/// </param>
	/// <returns>The HTML for one listing card.</returns>
	protected static string CreateCard(
		string identifier, string title, string? company)
	{
		string html = "<table><tr><td><a class='font-bold' " +
			"href='https://www.linkedin.com/jobs/view/" + identifier + "/'>" +
			title + "</a>";
		if (company != null)
		{
			html += "<p class='line-clamp-1'>" + company +
				" &middot; Tokyo</p>";
		}

		return html + "</td></tr></table>";
	}

	/// <summary>
	/// Opens the library against the test database.
	/// </summary>
	/// <returns>The initialized library manager.</returns>
	protected LibraryManager CreateLibrary()
	{
		return LibraryManager.Initialize(TestDbPath);
	}

	/// <summary>
	/// Creates the import service for the test database.
	/// </summary>
	/// <returns>The importer configured with the LinkedIn parser.</returns>
	protected EmailImportService CreateImportService()
	{
		return new EmailImportService(
			() => new JobsDbContext(TestDbPath!), new LinkedInEmailParser());
	}

	/// <summary>
	/// Writes an HTML export with sender and timestamp metadata.
	/// </summary>
	/// <param name="filename">
	/// The filename within the test import directory.
	/// </param>
	/// <param name="html">The HTML message body.</param>
	/// <param name="sender">
	/// The sender address to put in the export metadata.
	/// </param>
	protected void WriteMessage(
		string filename,
		string html,
		string sender = "jobalerts-noreply@linkedin.com")
	{
		string metadata = "<!-- Sent: 2026-11-17T09:00:00Z\nSender: " +
			sender + " -->\n\n";
		string path = Path.Combine(ImportDirectory, filename);
		File.WriteAllText(path, metadata + html);
	}
}
