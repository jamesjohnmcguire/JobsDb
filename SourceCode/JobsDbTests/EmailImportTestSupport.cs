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

internal abstract class EmailImportTestSupport : BaseTestsSupport
{
	protected string ImportDirectory { get; private set; } = string.Empty;

	[SetUp]
	public void SetUpImport()
	{
		DatabaseTestSchema.Create(TestDbPath!);
		ImportDirectory = TestDbPath + ".email batch";
		Directory.CreateDirectory(ImportDirectory);
	}

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

	protected LibraryManager CreateLibrary()
	{
		return LibraryManager.Initialize(TestDbPath);
	}

	protected EmailImportService CreateImportService()
	{
		return new EmailImportService(
			() => new JobsDbContext(TestDbPath!), new LinkedInEmailParser());
	}

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
