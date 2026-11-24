/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Coordinates offline extraction and a separate write scope per message.
/// </summary>
internal sealed class EmailImportService : IEmailImportService
{
	private readonly Func<JobsDbContext> createContext;
	private readonly IEmailJobParser parser;

	/// <summary>
	/// Initializes a new instance of the <see cref="EmailImportService"/>
	/// class.
	/// </summary>
	/// <param name="createContext">
	/// Creates a database context for each message write.
	/// </param>
	/// <param name="parser">
	/// Extracts job candidates from a supported message.
	/// </param>
	public EmailImportService(
		Func<JobsDbContext> createContext, IEmailJobParser parser)
	{
		this.createContext = createContext;
		this.parser = parser;
	}

	private enum CandidateStatus
	{
		Added,
		Updated,
		Unchanged,
		NeedsReview
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyList<EmailImportResult>> ImportDirectoryAsync(
		string directoryPath)
	{
		IReadOnlyList<EmailReadResult> emails =
			ExportedEmailReader.ReadDirectory(directoryPath);
		List<EmailImportResult> results = new List<EmailImportResult>();
		foreach (EmailReadResult email in emails)
		{
			EmailImportResult result =
				await ImportEmailAsync(email).ConfigureAwait(false);
			results.Add(result);
		}

		return results;
	}

	private static EmailImportResult NoWrites(
		string sourceName, EmailImportStatus status, string? message)
	{
		return new EmailImportResult(sourceName, status, 0, 0, 0, message);
	}

	private static EmailImportResult MapReadFailure(EmailReadResult result)
	{
		EmailImportStatus status = EmailImportStatus.Failed;
		if (result.Status == EmailReadStatus.Ignored)
		{
			status = EmailImportStatus.Ignored;
		}

		return NoWrites(result.SourceName, status, result.Message);
	}

	private static async Task<List<CandidateStatus>> WriteCandidatesAsync(
		IJobRepository repository, IReadOnlyList<EmailJobCandidate> candidates)
	{
		List<CandidateStatus> outcomes = new List<CandidateStatus>();
		foreach (EmailJobCandidate candidate in candidates)
		{
			CandidateStatus outcome =
				await ImportCandidateAsync(repository, candidate)
					.ConfigureAwait(false);
			outcomes.Add(outcome);
		}

		return outcomes;
	}

	private static async Task<CandidateStatus> ImportCandidateAsync(
		IJobRepository repository, EmailJobCandidate candidate)
	{
		string? company = candidate.Company;
		if (string.IsNullOrWhiteSpace(company))
		{
			return CandidateStatus.NeedsReview;
		}

		Job? existing = await repository.GetBySourceIdAsync(
			candidate.Source, candidate.SourceJobId).ConfigureAwait(false);
		if (existing != null)
		{
			return await UpdateExistingAsync(
				repository, existing, candidate, company)
				.ConfigureAwait(false);
		}

		Job job = CreateJob(candidate, company);
		await repository.AddAsync(job).ConfigureAwait(false);
		return CandidateStatus.Added;
	}

	private static Job CreateJob(EmailJobCandidate candidate, string company)
	{
		return new Job
		{
			Title = candidate.Title,
			Company = company,
			Location = candidate.Location,
			Source = candidate.Source,
			SourceJobId = candidate.SourceJobId,
			SourceUrl = candidate.SourceUrl.AbsoluteUri,
			DatePosted = candidate.DatePosted,
			Status = ApplicationStatus.NotApplied
		};
	}

	private static bool HasSummaryChanges(
		Job existing, EmailJobCandidate candidate)
	{
		bool locationChanged = !string.IsNullOrWhiteSpace(candidate.Location) &&
			existing.Location != candidate.Location;
		return existing.Title != candidate.Title ||
			existing.Company != candidate.Company ||
			existing.SourceUrl != candidate.SourceUrl.AbsoluteUri ||
			locationChanged;
	}

	private static void ApplySummary(
		Job existing, EmailJobCandidate candidate, string company)
	{
		existing.Title = candidate.Title;
		existing.Company = company;
		existing.SourceUrl = candidate.SourceUrl.AbsoluteUri;
		if (!string.IsNullOrWhiteSpace(candidate.Location))
		{
			existing.Location = candidate.Location;
		}

		existing.DateScraped = DateTime.UtcNow;
	}

	private static async Task<CandidateStatus> UpdateExistingAsync(
		IJobRepository repository,
		Job existing,
		EmailJobCandidate candidate,
		string company)
	{
		if (!HasSummaryChanges(existing, candidate))
		{
			return CandidateStatus.Unchanged;
		}

		ApplySummary(existing, candidate, company);
		await repository.UpdateAsync(existing).ConfigureAwait(false);
		return CandidateStatus.Updated;
	}

	private static EmailImportResult CreateResult(
		string sourceName,
		EmailParseResult parsed,
		List<CandidateStatus> outcomes)
	{
		EmailImportStatus status = EmailImportStatus.Completed;
		List<string> messages = new List<string>(parsed.Messages);
		if (outcomes.Contains(CandidateStatus.NeedsReview))
		{
			status = EmailImportStatus.NeedsReview;
			messages.Add("Candidates without a company require review.");
		}

		bool hasEstimatedDates = parsed.Candidates.Any(
			candidate => candidate.IsPostingDateEstimated);
		if (hasEstimatedDates)
		{
			messages.Add("Posting dates are estimates from email timestamps.");
		}

		return Summarize(sourceName, status, outcomes, messages);
	}

	private static EmailImportResult Summarize(
		string sourceName,
		EmailImportStatus status,
		List<CandidateStatus> outcomes,
		List<string> messages)
	{
		return new EmailImportResult(
			sourceName,
			status,
			outcomes.Count(outcome => outcome == CandidateStatus.Added),
			outcomes.Count(outcome => outcome == CandidateStatus.Updated),
			outcomes.Count(outcome => outcome == CandidateStatus.Unchanged),
			string.Join("; ", messages));
	}

	private async Task<EmailImportResult> ImportEmailAsync(
		EmailReadResult source)
	{
		if (source.Status != EmailReadStatus.Read || source.Email == null)
		{
			return MapReadFailure(source);
		}

		try
		{
			EmailParseResult parsed = parser.Parse(source.Email);
			if (parsed.Status == EmailParseStatus.Ignored)
			{
				return NoWrites(
					source.SourceName,
					EmailImportStatus.Ignored,
					string.Join("; ", parsed.Messages));
			}

			return await SaveCandidatesAsync(source.SourceName, parsed)
				.ConfigureAwait(false);
		}
		catch (Exception exception) when (
			exception is DbUpdateException or SqliteException or
				InvalidOperationException)
		{
			return NoWrites(
				source.SourceName, EmailImportStatus.Failed, exception.Message);
		}
	}

	private async Task<EmailImportResult> SaveCandidatesAsync(
		string sourceName, EmailParseResult parsed)
	{
		using JobsDbContext context = createContext();
		using IDbContextTransaction transaction =
			await context.Database.BeginTransactionAsync()
				.ConfigureAwait(false);
		JobRepository repository = new JobRepository(context);
		List<CandidateStatus> outcomes =
			await WriteCandidatesAsync(repository, parsed.Candidates)
				.ConfigureAwait(false);
		await transaction.CommitAsync().ConfigureAwait(false);
		return CreateResult(sourceName, parsed, outcomes);
	}
}
