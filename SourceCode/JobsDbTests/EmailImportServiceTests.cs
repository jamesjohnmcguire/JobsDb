/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportServiceTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Covers email import, job updates, and per-message transactions.
/// </summary>
[TestFixture]
internal sealed class EmailImportServiceTests : EmailImportTestSupport
{
	/// <summary>
	/// Checks that an import is repeatable and visible to job queries.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_IsRepeatableAndVisibleToQueries()
	{
		string card = CreateCard("123", "Developer", "Company");
		WriteMessage("first.eml", card);
		WriteMessage("second.eml", card);
		using LibraryManager library = CreateLibrary();

		EmailImportBatchResult first =
			await LibraryManager.ImportEmailsAsync(ImportDirectory, TestDbPath)
				.ConfigureAwait(false);
		EmailImportBatchResult repeated =
			await LibraryManager.ImportEmailsAsync(ImportDirectory, TestDbPath)
				.ConfigureAwait(false);
		List<Job> jobs = await library.JobQueries.GetJobsAsync()
			.ConfigureAwait(false);

		Assert.That(first.Added, Is.EqualTo(1));
		Assert.That(first.Unchanged, Is.EqualTo(1));
		Assert.That(repeated.Added, Is.Zero);
		Assert.That(repeated.Updated, Is.Zero);
		Assert.That(repeated.Unchanged, Is.EqualTo(2));
		Assert.That(jobs, Has.Count.EqualTo(1));
		Assert.That(jobs[0].Title, Is.EqualTo("Developer"));
	}

	/// <summary>
	/// Verifies that the registered Indeed sender saves stable postings.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_RoutesIndeedAndReportsPartialAlert()
	{
		const string id = "0123456789abcdef";
		string organicUrl = "https://www.indeed.com/rc/clk/dl?jk=" + id;
		string sponsoredUrl = "https://www.indeed.com/pagead/clk/dl?ad=opaque";
		string template = "<table><tr><td><h2>" +
			"<a class='strong-text-link' href='{0}'>Engineer</a>" +
			"</h2></td></tr><tr><td><p>Company</p><p>Tokyo</p></td>" +
			"</tr></table>";
		string html = string.Format(
			CultureInfo.InvariantCulture, template, organicUrl);
		html += string.Format(
			CultureInfo.InvariantCulture, template, sponsoredUrl);
		WriteMessage("indeed.eml", html, IndeedEmailParser.JobAlertSender);

		EmailImportBatchResult first = await LibraryManager
			.ImportEmailsAsync(ImportDirectory, TestDbPath)
			.ConfigureAwait(false);
		EmailImportBatchResult repeated = await LibraryManager
			.ImportEmailsAsync(ImportDirectory, TestDbPath)
			.ConfigureAwait(false);

		Assert.That(first.Added, Is.EqualTo(1));
		Assert.That(first.NeedsReview, Is.EqualTo(1));
		Assert.That(repeated.Added, Is.Zero);
		Assert.That(repeated.Unchanged, Is.EqualTo(1));
		string processed = Path.Combine(ImportDirectory, "processed.txt");
		string unprocessed = Path.Combine(ImportDirectory, "unprocessed.txt");
		Assert.That(File.ReadAllLines(processed), Is.Empty);
		string[] expected = { "indeed" };
		Assert.That(File.ReadAllLines(unprocessed), Is.EqualTo(expected));
		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Job stored = reader.Jobs.Single();
		Assert.That(stored.Source, Is.EqualTo("Indeed"));
		Assert.That(stored.SourceJobId, Is.EqualTo(id));
	}

	/// <summary>
	/// Checks that an import preserves richer details and application data.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_PreservesRicherAndApplicationData()
	{
		using JobsDbContext preparation = new JobsDbContext(TestDbPath!);
		JobRepository repository = new JobRepository(preparation);
		Job existing = CopyJob(TestJob!);
		existing.Source = "LinkedIn";
		existing.SourceJobId = "123";
		existing.Status = ApplicationStatus.Applied;
		existing.Notes = "Keep my notes";
		existing.Priority = 4;
		existing.IsArchived = true;
		existing.SalaryMin = 123000;
		DateTime originalDate = existing.DatePosted;
		await repository.AddAsync(existing).ConfigureAwait(false);
		WriteMessage("message.eml", CreateCard("123", "New Title", "New Co"));

		EmailImportBatchResult results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Job stored = reader.Jobs.Single();
		Assert.That(results.Results.Single().Updated, Is.EqualTo(1));
		Assert.That(stored.Title, Is.EqualTo("New Title"));
		Assert.That(stored.Company, Is.EqualTo("New Co"));
		Assert.That(stored.Description, Is.EqualTo(TestJob!.Description));
		Assert.That(stored.Requirements, Is.EqualTo(TestJob.Requirements));
		Assert.That(stored.SalaryMin, Is.EqualTo(123000));
		Assert.That(stored.Notes, Is.EqualTo("Keep my notes"));
		Assert.That(stored.Status, Is.EqualTo(ApplicationStatus.Applied));
		Assert.That(stored.Priority, Is.EqualTo(4));
		Assert.That(stored.IsArchived, Is.True);
		Assert.That(stored.DatePosted, Is.EqualTo(originalDate));
	}

	/// <summary>
	/// Checks that ignored, failed, and review items are reported separately.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_ReportsIgnoredFailedAndReviewItems()
	{
		WriteMessage("valid.eml", CreateCard("123", "Developer", "Company"));
		WriteMessage("review.eml", CreateCard("456", "Engineer", null));
		WriteMessage(
			"other.eml", "<body>Other provider</body>", "jobs@dice.com");
		string partial = CreateCard("987", "Architect", "Company") +
			"<a class='font-bold' " +
			"href='https://www.linkedin.com/jobs/view/7'>Incomplete card</a>";
		WriteMessage("partial.eml", partial);
		string processed = Path.Combine(ImportDirectory, "processed.txt");
		File.WriteAllText(processed, "stale-id");
		await File.WriteAllTextAsync(
			Path.Combine(ImportDirectory, "unknown.eml"), "Unknown")
			.ConfigureAwait(false);
		await File.WriteAllTextAsync(
			Path.Combine(ImportDirectory, "broken.eml"),
			"From: jobs@example.com\r\nDate: bad timestamp\r\n\r\nBody")
			.ConfigureAwait(false);

		EmailImportBatchResult results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		Assert.That(results.Ignored, Is.EqualTo(1));
		Assert.That(results.Failed, Is.EqualTo(2));
		Assert.That(results.NeedsReview, Is.EqualTo(2));
		Assert.That(results.Added, Is.EqualTo(2));
		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Count(), Is.EqualTo(2));
		string unprocessed = Path.Combine(ImportDirectory, "unprocessed.txt");
		string[] accepted = { "valid" };
		string[] rejected =
		{
			"broken", "other", "partial", "review", "unknown"
		};
		Assert.That(File.ReadAllLines(processed), Is.EqualTo(accepted));
		Assert.That(File.ReadAllLines(unprocessed), Is.EqualTo(rejected));
	}

	/// <summary>
	/// Checks that an absent incoming location preserves the stored location.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_MissingLocationPreservesStoredValue()
	{
		using JobsDbContext preparation = new JobsDbContext(TestDbPath!);
		JobRepository repository = new JobRepository(preparation);
		Job existing = CopyJob(TestJob!);
		existing.Source = "LinkedIn";
		existing.SourceJobId = "123";
		existing.Location = "Keep location";
		await repository.AddAsync(existing).ConfigureAwait(false);
		string html = CreateCard("123", "New title", "Company");
		html = html.Replace(
			" &middot; Tokyo", string.Empty, StringComparison.Ordinal);
		WriteMessage("message.eml", html);

		await CreateImportService().ImportDirectoryAsync(ImportDirectory)
			.ConfigureAwait(false);

		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Single().Location, Is.EqualTo("Keep location"));
	}

	/// <summary>
	/// Checks that a failed message rolls back and later messages still import.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task ImportDirectory_RollsBackFailedMessageAndContinues()
	{
		using (JobsDbContext preparation = new JobsDbContext(TestDbPath!))
		{
			const string trigger = "CREATE TRIGGER RejectJob BEFORE INSERT " +
				"ON Jobs WHEN NEW.sourceJobId = '456' BEGIN " +
				"SELECT RAISE(ABORT, 'Rejected test job'); END;";
			await preparation.Database.ExecuteSqlRawAsync(trigger)
				.ConfigureAwait(false);
		}

		string rejectedMessage = CreateCard("123", "First", "Company") +
			CreateCard("456", "Rejected", "Company");
		WriteMessage("first.eml", rejectedMessage);
		WriteMessage("second.eml", CreateCard("789", "Accepted", "Company"));

		EmailImportBatchResult results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		Assert.That(
			results.Results[0].Status, Is.EqualTo(EmailImportStatus.Failed));
		Assert.That(results.Results[0].Added, Is.Zero);
		Assert.That(results.Results[1].Added, Is.EqualTo(1));
		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Single().SourceJobId, Is.EqualTo("789"));
	}
}
