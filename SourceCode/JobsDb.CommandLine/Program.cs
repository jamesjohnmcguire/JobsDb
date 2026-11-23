/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.CommandLine;

using System;
using System.IO;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using LoggingService;
using Serilog;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		if (!CommandRunner.IsImportCommand(args))
		{
			return await CreateRunner().RunAsync(args).ConfigureAwait(false);
		}

		ConfigureImportLogging();
		try
		{
			CommandRunner runner = CreateRunner(Log.Logger);
			return await runner.RunAsync(args).ConfigureAwait(false);
		}
		finally
		{
			await Log.CloseAndFlushAsync().ConfigureAwait(false);
		}
	}

	private static CommandRunner CreateRunner(ILogger? logger = null)
	{
		return new CommandRunner(
			Console.Out,
			Console.Error,
			() => LibraryManager.Initialize(),
			logger);
	}

	private static void ConfigureImportLogging()
	{
		string applicationData = Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData);
		string logPath = Path.Combine(
			applicationData, "JobsDb", "Logs", "import-.log");
		LogService.Configure(logPath);
	}
}
