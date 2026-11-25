/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportServiceTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
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
		WriteMessage("first.html", card);
		WriteMessage("second.html", card);
		using LibraryManager library = CreateLibrary();

		IReadOnlyList<EmailImportResult> first =
			await library.EmailImports.ImportDirectoryAsync(ImportDirectory)
				.ConfigureAwait(false);
		IReadOnlyList<EmailImportResult> repeated =
			await library.EmailImports.ImportDirectoryAsync(ImportDirectory)
				.ConfigureAwait(false);
		List<Job> jobs = await library.JobQueries.GetJobsAsync()
			.ConfigureAwait(false);

		Assert.That(first.Sum(result => result.Added), Is.EqualTo(1));
		Assert.That(first.Sum(result => result.Unchanged), Is.EqualTo(1));
		Assert.That(repeated.Sum(result => result.Added), Is.Zero);
		Assert.That(repeated.Sum(result => result.Updated), Is.Zero);
		Assert.That(repeated.Sum(result => result.Unchanged), Is.EqualTo(2));
		Assert.That(jobs, Has.Count.EqualTo(1));
		Assert.That(jobs[0].Title, Is.EqualTo("Developer"));
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
		WriteMessage("message.html", CreateCard("123", "New Title", "New Co"));

		IReadOnlyList<EmailImportResult> results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Job stored = reader.Jobs.Single();
		Assert.That(results.Single().Updated, Is.EqualTo(1));
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
		WriteMessage("valid.html", CreateCard("123", "Developer", "Company"));
		WriteMessage("review.html", CreateCard("456", "Engineer", null));
		WriteMessage(
			"other.html", "<body>Other provider</body>", "jobs@dice.com");
		await File.WriteAllTextAsync(
			Path.Combine(ImportDirectory, "unknown.eml"), "Unknown")
			.ConfigureAwait(false);
		await File.WriteAllTextAsync(
			Path.Combine(ImportDirectory, "broken.txt"),
			"Sent: bad timestamp\nSender: jobs@example.com")
			.ConfigureAwait(false);

		IReadOnlyList<EmailImportResult> results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		int ignored = results.Count(
			result => result.Status == EmailImportStatus.Ignored);
		int failed = results.Count(
			result => result.Status == EmailImportStatus.Failed);
		int review = results.Count(
			result => result.Status == EmailImportStatus.NeedsReview);
		Assert.That(ignored, Is.EqualTo(1));
		Assert.That(failed, Is.EqualTo(2));
		Assert.That(review, Is.EqualTo(1));
		Assert.That(results.Sum(result => result.Added), Is.EqualTo(1));
		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Count(), Is.EqualTo(1));
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
		WriteMessage("message.html", html);

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
		WriteMessage("first.html", rejectedMessage);
		WriteMessage("second.html", CreateCard("789", "Accepted", "Company"));

		IReadOnlyList<EmailImportResult> results = await CreateImportService()
			.ImportDirectoryAsync(ImportDirectory).ConfigureAwait(false);

		Assert.That(results[0].Status, Is.EqualTo(EmailImportStatus.Failed));
		Assert.That(results[0].Added, Is.Zero);
		Assert.That(results[1].Added, Is.EqualTo(1));
		using JobsDbContext reader = new JobsDbContext(TestDbPath!);
		Assert.That(reader.Jobs.Single().SourceJobId, Is.EqualTo("789"));
	}
}
