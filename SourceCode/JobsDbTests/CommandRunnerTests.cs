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
internal sealed class CommandRunnerTests : EmailImportTestSupport
{
	/// <summary>
	/// Verifies that help works without opening a database.
	/// </summary>
	/// <param name="command">
	/// The help command.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase("help")]
	[TestCase("HELP")]
	public async Task Help_DoesNotInitializeDatabase(string command)
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);

		string unusedDatabase = Path.Combine(ImportDirectory, "unused.db");
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, unusedDatabase);
		int result = await runner.RunAsync(new[] { command })
			.ConfigureAwait(false);

		Assert.That(result, Is.Zero);
		Assert.That(
			output.ToString(), Does.Contain("jobsdb import <directory>"));
		Assert.That(File.Exists(unusedDatabase), Is.False);
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

		string unusedDatabase = Path.Combine(ImportDirectory, "unused.db");
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, unusedDatabase);

		string[] arguments = command.Split(
			' ', StringSplitOptions.RemoveEmptyEntries);
		int result = await runner.RunAsync(arguments)
			.ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(-1));
		Assert.That(error.ToString(), Does.Contain("Invalid command"));
		Assert.That(File.Exists(unusedDatabase), Is.False);
	}

	/// <summary>
	/// Verifies that the CLI saves listings and reports unchanged reimports.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Import_SavesJobsAndReportsRepeatRuns()
	{
		WriteMessage("message.eml", CreateCard("123", "Developer", "Company"));
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, TestDbPath);
		string[] arguments = { "import", ImportDirectory };

		int first = await runner.RunAsync(arguments).ConfigureAwait(false);
		string[] uppercase = { "IMPORT", ImportDirectory };
		int repeated = await runner.RunAsync(uppercase).ConfigureAwait(false);

		Assert.That(first, Is.Zero);
		Assert.That(repeated, Is.Zero);
		Assert.That(
			output.ToString(), Does.Contain("Completed message: added 1"));
		Assert.That(output.ToString(), Does.Contain("unchanged 1"));
		Assert.That(error.ToString(), Is.Empty);
	}

	/// <summary>
	/// Verifies that a missing import directory produces a failure exit code.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Import_MissingDirectoryReturnsFailure()
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, TestDbPath);
		string missing = Path.Combine(ImportDirectory, "missing");
		string[] arguments = { "import", missing };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(-1));
		Assert.That(error.ToString(), Is.Not.Empty);
	}

	/// <summary>
	/// Checks that unsupported messages are reported without a failed exit
	/// code.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Import_IgnoredMessagesReturnSuccessWithReason()
	{
		WriteMessage("other.eml", "<body>Other</body>", "jobs@indeed.com");
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, TestDbPath);
		string[] arguments = { "import", ImportDirectory };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.Zero);
		Assert.That(output.ToString(), Does.Contain("Ignored other:"));
		Assert.That(error.ToString(), Is.Empty);
	}

	/// <summary>
	/// Checks that a failed message yields an error while valid jobs are saved.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task Import_ItemFailureReturnsFailureAndKeepsValidJobs()
	{
		WriteMessage("valid.eml", CreateCard("123", "Developer", "Company"));
		WriteMessage("other.eml", "<body>Other</body>", "jobs@indeed.com");
		WriteMessage("review.eml", CreateCard("456", "Engineer", null));
		string partial = CreateCard("987", "Architect", "Company") +
			"<a class='font-bold' " +
			"href='https://www.linkedin.com/jobs/view/7'>" +
			"Incomplete card</a>";
		WriteMessage("partial.eml", partial);
		string stale = Path.Combine(ImportDirectory, "processed.txt");
		File.WriteAllText(stale, "stale-id");
		string path = Path.Combine(ImportDirectory, "broken.eml");
		string malformed = "From: jobs@x.com\r\nDate: invalid\r\n\r\nBody";
		await File.WriteAllTextAsync(path, malformed)
			.ConfigureAwait(false);
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, Logger.None, TestDbPath);
		string[] arguments = { "import", ImportDirectory };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(-1));
		Assert.That(
			output.ToString(), Does.Contain("Completed valid: added 1"));
		Assert.That(
			output.ToString(), Does.Contain("NeedsReview partial: added 1"));
		Assert.That(error.ToString(), Does.Contain("Invalid EML"));
		string processed = Path.Combine(ImportDirectory, "processed.txt");
		string unprocessed = Path.Combine(ImportDirectory, "unprocessed.txt");
		string[] accepted = { "valid" };
		Assert.That(File.ReadAllLines(processed), Is.EqualTo(accepted));
		string[] rejected = { "broken", "other", "partial", "review" };
		Assert.That(File.ReadAllLines(unprocessed), Is.EqualTo(rejected));
	}
}
