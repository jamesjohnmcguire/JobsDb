/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportBatchResult.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Summarizes a batch of committed and unsuccessful message imports.
/// </summary>
public sealed class EmailImportBatchResult
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EmailImportBatchResult"/>
	/// class.
	/// </summary>
	/// <param name="results">The outcome for each message.</param>
	public EmailImportBatchResult(IReadOnlyList<EmailImportResult> results)
	{
		Results = Array.AsReadOnly(results.ToArray());
		Added = Results.Sum(result => result.Added);
		Updated = Results.Sum(result => result.Updated);
		Unchanged = Results.Sum(result => result.Unchanged);
		Ignored = Results.Count(
			result => result.Status == EmailImportStatus.Ignored);
		NeedsReview = Results.Count(
			result => result.Status == EmailImportStatus.NeedsReview);
		Failed = Results.Count(
			result => result.Status == EmailImportStatus.Failed);
	}

	/// <summary>Gets the individual message outcomes.</summary>
	public IReadOnlyList<EmailImportResult> Results { get; }

	/// <summary>Gets the number of jobs added.</summary>
	public int Added { get; }

	/// <summary>Gets the number of jobs updated.</summary>
	public int Updated { get; }

	/// <summary>Gets the number of jobs left unchanged.</summary>
	public int Unchanged { get; }

	/// <summary>Gets the number of messages skipped.</summary>
	public int Ignored { get; }

	/// <summary>Gets the number of messages requiring review.</summary>
	public int NeedsReview { get; }

	/// <summary>Gets the number of messages that failed.</summary>
	public int Failed { get; }

	/// <summary>Gets a value indicating whether any message failed.</summary>
	public bool HasFailures => Failed > 0;

	/// <summary>Gets a summary of the job changes.</summary>
	public string JobSummary =>
		$"Jobs: {Added} added, {Updated} updated, {Unchanged} unchanged.";

	/// <summary>Gets a summary of the message outcomes.</summary>
	public string MessageSummary =>
		$"Messages: {Ignored} ignored, " +
		$"{NeedsReview} need review, {Failed} failed.";
}
