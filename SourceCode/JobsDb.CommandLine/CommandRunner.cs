/////////////////////////////////////////////////////////////////////////////
// <copyright file="CommandRunner.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.CommandLine;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
	private readonly ILogger logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandRunner"/> class.
	/// </summary>
	/// <param name="output">The writer for command output.</param>
	/// <param name="error">The writer for command errors.</param>
	/// <param name="logger">The logger for import summaries.</param>
	public CommandRunner(
		TextWriter output,
		TextWriter error,
		ILogger logger)
	{
		this.output = output;
		this.error = error;
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

			EmailImportBatchResult batch = await LibraryManager
				.ImportEmailsAsync(directory).ConfigureAwait(false);
			WriteResults(batch);
			if (batch.HasFailures)
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

	private void WriteResults(EmailImportBatchResult batch)
	{
		foreach (EmailImportResult result in batch.Results)
		{
			TextWriter writer = output;
			if (result.Status == EmailImportStatus.Failed)
			{
				writer = error;
			}

			writer.WriteLine(result.Summary);
			if (!string.IsNullOrEmpty(result.Message))
			{
				writer.WriteLine("  " + result.Message);
			}
		}

		logger.Information("{ImportSummary}", batch.JobSummary);
		LogEventLevel level = LogEventLevel.Information;
		if (batch.HasFailures)
		{
			level = LogEventLevel.Error;
		}

		logger.Write(level, "{ImportSummary}", batch.MessageSummary);
	}
}
