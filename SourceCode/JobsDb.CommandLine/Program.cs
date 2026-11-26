/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.CommandLine;

using System;
using System.IO;
using System.Threading.Tasks;
using LoggingService;
using Serilog;

/// <summary>
/// Configures import logging and runs the requested JobsDb command.
/// </summary>
internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		ConfigureImportLogging();

		try
		{
			CommandRunner runner = new CommandRunner(
				Console.Out, Console.Error, Log.Logger);
			return await runner.RunAsync(args).ConfigureAwait(false);
		}
		finally
		{
			await Log.CloseAndFlushAsync().ConfigureAwait(false);
		}
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
