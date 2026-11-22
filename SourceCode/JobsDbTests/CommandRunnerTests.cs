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
using DigitalZenWorks.JobsDb.Library;
using JobsDb.CommandLine;
using NUnit.Framework;

[TestFixture]
internal sealed class CommandRunnerTests : EmailImportTestSupport
{
	[TestCase("")]
	[TestCase("help")]
	public async Task Help_DoesNotInitializeDatabase(string command)
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, FailInitialization);
		string[] arguments = Array.Empty<string>();
		if (command.Length > 0)
		{
			arguments = new[] { command };
		}

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.Zero);
		Assert.That(
			output.ToString(), Does.Contain("jobsdb import <directory>"));
		Assert.That(error.ToString(), Is.Empty);
	}

	[TestCase("unknown")]
	[TestCase("import")]
	public async Task InvalidArguments_ReturnUsageError(string command)
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(
			output, error, FailInitialization);

		int result = await runner.RunAsync(new[] { command })
			.ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(2));
		Assert.That(error.ToString(), Does.Contain("Invalid command"));
	}

	[Test]
	public async Task Import_SavesJobsAndReportsRepeatRuns()
	{
		WriteMessage("message.html", CreateCard("123", "Developer", "Company"));
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(output, error, CreateLibrary);
		string[] arguments = { "import", ImportDirectory };

		int first = await runner.RunAsync(arguments).ConfigureAwait(false);
		int repeated = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(first, Is.Zero);
		Assert.That(repeated, Is.Zero);
		Assert.That(output.ToString(), Does.Contain("1 added"));
		Assert.That(output.ToString(), Does.Contain("1 unchanged"));
		Assert.That(error.ToString(), Is.Empty);
	}

	[Test]
	public async Task Import_MissingDirectoryReturnsFailure()
	{
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(output, error, CreateLibrary);
		string missing = Path.Combine(ImportDirectory, "missing");
		string[] arguments = { "import", missing };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(1));
		Assert.That(error.ToString(), Is.Not.Empty);
	}

	[Test]
	public async Task Import_IgnoredMessagesReturnSuccessWithReason()
	{
		WriteMessage("other.html", "<body>Other</body>", "jobs@indeed.com");
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(output, error, CreateLibrary);
		string[] arguments = { "import", ImportDirectory };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.Zero);
		Assert.That(output.ToString(), Does.Contain("1 ignored"));
		Assert.That(error.ToString(), Is.Empty);
	}

	[Test]
	public async Task Import_ItemFailureReturnsFailureAndKeepsValidJobs()
	{
		WriteMessage("valid.html", CreateCard("123", "Developer", "Company"));
		string path = Path.Combine(ImportDirectory, "broken.txt");
		await File.WriteAllTextAsync(path, "Sent: invalid\nSender: jobs@x.com")
			.ConfigureAwait(false);
		using StringWriter output =
			new StringWriter(CultureInfo.InvariantCulture);
		using StringWriter error =
			new StringWriter(CultureInfo.InvariantCulture);
		CommandRunner runner = new CommandRunner(output, error, CreateLibrary);
		string[] arguments = { "import", ImportDirectory };

		int result = await runner.RunAsync(arguments).ConfigureAwait(false);

		Assert.That(result, Is.EqualTo(1));
		Assert.That(output.ToString(), Does.Contain("1 added"));
		Assert.That(error.ToString(), Does.Contain("Invalid Sent"));
	}

	private static LibraryManager FailInitialization()
	{
		throw new InvalidOperationException("Initialization must not run.");
	}
}
