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
using Serilog;
using Serilog.Events;

/// <summary>
/// Dispatches manual and scheduled command-line invocations.
/// </summary>
internal sealed class CommandRunner
{
	private readonly TextWriter output;
	private readonly TextWriter error;
	private readonly string? databasePath;
	private readonly ILogger logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandRunner"/> class.
	/// </summary>
	/// <param name="output">The writer for command output.</param>
	/// <param name="error">The writer for command errors.</param>
	/// <param name="logger">The logger for import summaries.</param>
	/// <param name="databasePath">
	/// The database file, or null to use the application database.
	/// </param>
	public CommandRunner(
		TextWriter output,
		TextWriter error,
		ILogger logger,
		string? databasePath = null)
	{
		this.output = output;
		this.error = error;
		this.databasePath = databasePath;
		this.logger = logger;
	}

	/// <summary>
	/// Runs the requested command and reports the result.
	/// </summary>
	/// <param name="args">The command-line arguments.</param>
	/// <returns>
	/// Zero for success or minus one for an error.
	/// </returns>
	public async Task<int> RunAsync(string[] args)
	{
		int returnCode = -1;

		string command = string.Empty;

		if (args.Length > 0)
		{
			command = args[0].ToLowerInvariant();
		}

		switch (command)
		{
			case "import" when args.Length == 2:
				string importPath = args[1];
				returnCode = await ImportAsync(importPath).ConfigureAwait(false);
				break;
			case "help" when args.Length == 1:
				WriteHelp(output);
				returnCode = 0;
				break;
			default:
				await error.WriteLineAsync("Invalid command or arguments.")
					.ConfigureAwait(false);
				WriteHelp(output);
				break;
		}

		return returnCode;
	}

	private static void WriteHelp(TextWriter writer)
	{
		writer.WriteLine("jobsdb import <directory>");
		writer.WriteLine("jobsdb help");
	}

	[SuppressMessage(
		"Design",
		"CA1031",
		Justification = "Reports command failures with exit code -1.")]
	private async Task<int> ImportAsync(string directory)
	{
		try
		{
			logger.Information("Email import started.");

			using LibraryManager library =
				LibraryManager.Initialize(databasePath);
			IReadOnlyList<EmailImportResult> results =
				await library.EmailImports.ImportDirectoryAsync(directory)
					.ConfigureAwait(false);
			WriteResults(results);
			bool hasFailures = results.Any(
				result => result.Status == EmailImportStatus.Failed);
			if (hasFailures)
			{
				return -1;
			}

			return 0;
		}
		catch (Exception exception)
		{
			logger.Error(
				"Email import stopped with {ErrorType}.",
				exception.GetType().Name);
			await error.WriteLineAsync(exception.Message).ConfigureAwait(false);
			return -1;
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
		string totals =
			$"Jobs: {added} added, {updated} updated, {unchanged} unchanged.";
		WriteSummary(totals, LogEventLevel.Information);
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
		LogEventLevel level = failed > 0
			? LogEventLevel.Error
			: LogEventLevel.Information;
		WriteSummary(counts, level);
	}

	private void WriteSummary(string summary, LogEventLevel level)
	{
		logger.Write(level, "{ImportSummary}", summary);
	}
}
