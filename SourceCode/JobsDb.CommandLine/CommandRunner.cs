/////////////////////////////////////////////////////////////////////////////
// <copyright file="CommandRunner.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.CommandLine;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.EmailImport;

/// <summary>
/// Dispatches manual and scheduled command-line invocations.
/// </summary>
internal sealed class CommandRunner
{
	private readonly TextWriter output;
	private readonly TextWriter error;
	private readonly Func<LibraryManager> initialize;

	public CommandRunner(
		TextWriter output, TextWriter error, Func<LibraryManager> initialize)
	{
		this.output = output;
		this.error = error;
		this.initialize = initialize;
	}

	public async Task<int> RunAsync(string[] args)
	{
		if (args.Length == 0 ||
			(args.Length == 1 && args[0].Equals(
				"help", StringComparison.OrdinalIgnoreCase)))
		{
			WriteHelp(output);
			return 0;
		}

		if (args.Length == 2 &&
			args[0].Equals("import", StringComparison.OrdinalIgnoreCase))
		{
			return await ImportAsync(args[1]).ConfigureAwait(false);
		}

		await error.WriteLineAsync("Invalid command or arguments.")
			.ConfigureAwait(false);
		WriteHelp(error);
		return 2;
	}

	private static void WriteHelp(TextWriter writer)
	{
		writer.WriteLine("jobsdb import <directory>");
		writer.WriteLine("jobsdb help");
	}

	[SuppressMessage(
		"Design",
		"CA1031",
		Justification = "Reports command failures with exit code 1.")]
	private async Task<int> ImportAsync(string directory)
	{
		try
		{
			using LibraryManager library = initialize();
			IReadOnlyList<EmailImportResult> results =
				await library.EmailImports.ImportDirectoryAsync(directory)
					.ConfigureAwait(false);
			WriteResults(results);
			bool hasFailures = results.Any(
				result => result.Status == EmailImportStatus.Failed);
			if (hasFailures)
			{
				return 1;
			}

			return 0;
		}
		catch (Exception exception)
		{
			await error.WriteLineAsync(exception.Message).ConfigureAwait(false);
			return 1;
		}
	}

	private void WriteResults(IReadOnlyList<EmailImportResult> results)
	{
		foreach (EmailImportResult result in results)
		{
			WriteResult(result);
		}

		int added = results.Sum(result => result.Added);
		int updated = results.Sum(result => result.Updated);
		int unchanged = results.Sum(result => result.Unchanged);
		output.WriteLine(
			$"Jobs: {added} added, {updated} updated, {unchanged} unchanged.");
		WriteMessageCounts(results);
	}

	private void WriteResult(EmailImportResult result)
	{
		TextWriter writer = output;
		if (result.Status == EmailImportStatus.Failed)
		{
			writer = error;
		}

		string counts = $"added {result.Added}, updated {result.Updated}, " +
			$"unchanged {result.Unchanged}";
		writer.WriteLine($"{result.Status} {result.SourceName}: {counts}");
		if (!string.IsNullOrEmpty(result.Message))
		{
			writer.WriteLine("  " + result.Message);
		}
	}

	private void WriteMessageCounts(IReadOnlyList<EmailImportResult> results)
	{
		int ignored = results.Count(
			result => result.Status == EmailImportStatus.Ignored);
		int review = results.Count(
			result => result.Status == EmailImportStatus.NeedsReview);
		int failed = results.Count(
			result => result.Status == EmailImportStatus.Failed);
		string counts = $"Messages: {ignored} ignored, " +
			$"{review} need review, {failed} failed.";
		output.WriteLine(counts);
	}
}
