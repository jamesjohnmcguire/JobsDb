/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.CommandLine;

using System;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		CommandRunner runner = new CommandRunner(
			Console.Out, Console.Error, () => LibraryManager.Initialize());
		return await runner.RunAsync(args).ConfigureAwait(false);
	}
}
