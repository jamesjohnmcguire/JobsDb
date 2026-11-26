/////////////////////////////////////////////////////////////////////////////
// <copyright file="CommandRunnerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using JobsDb.CommandLine;
using NUnit.Framework;
using Serilog.Core;

/// <summary>
/// Covers command dispatch, import outcomes, and exit codes.
/// </summary>
[TestFixture]
internal sealed class CommandRunnerTests
{
	/// <summary>
	/// Verifies that help prints usage successfully.
	/// </summary>
	/// <param name="command">
	/// The help command.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase("help")]
	[TestCase("HELP")]
	public async Task Help_PrintsUsage(string command)
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);

		CommandRunner runner = new CommandRunner(output, error, Logger.None);
		int result = await runner.RunAsync(new[] { command })
			.ConfigureAwait(false);

		Assert.That(result, Is.Zero);
		Assert.That(
			output.ToString(), Does.Contain("jobsdb import <directory>"));
		Assert.That(error.ToString(), Is.Empty);
	}

	/// <summary>
	/// Verifies that invalid or incomplete commands report a usage error.
	/// </summary>
	/// <param name="command">The invalid or incomplete command.</param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase("")]
	[TestCase("unknown")]
	[TestCase("import")]
	[TestCase("help extra")]
	[TestCase("import first second")]
	public async Task InvalidArguments_ReturnUsageError(string command)
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);

		CommandRunner runner = new CommandRunner(output, error, Logger.None);

		string[] arguments = command.Split(
			' ', StringSplitOptions.RemoveEmptyEntries);
		int result = await runner.RunAsync(arguments)
			.ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(-1));
		Assert.That(error.ToString(), Does.Contain("Invalid command"));
	}

	/// <summary>
	/// Checks that import reports a rejected directory and returns failure.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Import_BlankDirectoryReturnsFailure()
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(output, error, Logger.None);
		string[] arguments = { "IMPORT", " " };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(-1));
		Assert.That(error.ToString(), Does.Contain("directoryPath"));
		Assert.That(output.ToString(), Is.Empty);
	}
}
