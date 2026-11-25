/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExportedEmailReaderTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using NUnit.Framework;

/// <summary>
/// Covers local email export pairing, metadata, and read outcomes.
/// </summary>
[TestFixture]
internal sealed class ExportedEmailReaderTests
{
	private string directoryPath = string.Empty;

	/// <summary>
	/// Creates the temporary directory for email exports.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		directoryPath = Path.Combine(
			Path.GetTempPath(), "JobsDb-email-reader-" + Guid.NewGuid());
		Directory.CreateDirectory(directoryPath);
	}

	/// <summary>
	/// Removes the export fixtures and their directory.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		foreach (string path in Directory.GetFiles(directoryPath))
		{
			File.Delete(path);
		}

		Directory.Delete(directoryPath);
	}

	/// <summary>
	/// Checks that paired exports preserve both bodies.
	/// </summary>
	/// <param name="newline">
	/// The line ending to use in the export files.
	/// </param>
	[TestCase("\n")]
	[TestCase("\r\n")]
	public void ReadDirectory_PairsBodiesAndPreservesContent(string newline)
	{
		string headers = "Sent: 2026-11-17T01:16:34.0000000" + newline +
			"Sender: リモグ & Tools <jobs@example.com>";
		string textBody = "  【開発者】\nKeep these blank lines.\n\nEnd";
		string htmlBody = "<html><body>【開発者】 &amp; tools</body></html>";
		string encodedHeaders = headers
			.Replace("&", "&amp;", StringComparison.Ordinal)
			.Replace("<", "&lt;", StringComparison.Ordinal)
			.Replace(">", "&gt;", StringComparison.Ordinal);
		string originalText = headers + newline + newline + textBody;
		WriteFile("message.txt", originalText);
		WriteFile(
			"message.html",
			"<!-- " + encodedHeaders + " -->" + newline + newline + htmlBody);

		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Read));
		Assert.That(result.SourceName, Is.EqualTo("message"));
		Assert.That(result.Email!.SenderName, Is.EqualTo("リモグ & Tools"));
		Assert.That(
			result.Email.SenderAddress, Is.EqualTo("jobs@example.com"));
		DateTime expected = new DateTime(2026, 11, 17, 1, 16, 34);
		Assert.That(result.Email.SentOn, Is.EqualTo(expected));
		Assert.That(
			result.Email.SentOn.Kind, Is.EqualTo(DateTimeKind.Unspecified));
		Assert.That(result.Email.HtmlBody, Is.EqualTo(htmlBody));
		Assert.That(result.Email.TextBody, Is.EqualTo(textBody));
		Assert.That(Directory.GetFiles(directoryPath), Has.Length.EqualTo(2));
		string unchanged = File.ReadAllText(
			Path.Combine(directoryPath, "message.txt"));
		Assert.That(unchanged, Is.EqualTo(originalText));
	}

	/// <summary>
	/// Checks that a single supported body file can be read.
	/// </summary>
	/// <param name="extension">The body file extension.</param>
	/// <param name="isHtml">Whether the file contains HTML.</param>
	[TestCase(".HTML", true)]
	[TestCase(".TXT", false)]
	public void ReadDirectory_AcceptsOneBody(string extension, bool isHtml)
	{
		string content = "Sent: 2026-11-17T01:16:34\n" +
			"Sender: Jobs <jobs@example.com>";
		if (isHtml)
		{
			string headers = content.Replace(
				"<jobs@example.com>",
				"&lt;jobs@example.com&gt;",
				StringComparison.Ordinal);
			content = "<!-- " + headers + " -->\n\n<body>Listing</body>";
		}
		else
		{
			content += "\n\nListing";
		}

		WriteFile("message" + extension, content);
		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Read));
		if (isHtml)
		{
			Assert.That(result.Email!.TextBody, Is.Null);
			Assert.That(result.Email.HtmlBody, Does.Contain("Listing"));
		}
		else
		{
			Assert.That(result.Email!.HtmlBody, Is.Null);
			Assert.That(result.Email.TextBody, Is.EqualTo("Listing"));
		}
	}

	/// <summary>
	/// Checks that timestamps with known offsets normalize to UTC.
	/// </summary>
	/// <param name="timestamp">The timestamp with an explicit offset.</param>
	[TestCase("2026-11-17T09:00:00Z")]
	[TestCase("2026-11-17T18:00:00+09:00")]
	public void ReadDirectory_NormalizesKnownOffsetsToUtc(string timestamp)
	{
		string content = "Sent: " + timestamp +
			"\nSender: Jobs <jobs@example.com>\n\nListing";
		WriteFile("message.txt", content);

		EmailReadResult result = ReadSingle();

		DateTime expected = new DateTime(
			2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Read));
		Assert.That(result.Email!.SentOn, Is.EqualTo(expected));
		Assert.That(result.Email.SentOn.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	/// <summary>
	/// Checks that conflicting headers in paired exports are reported.
	/// </summary>
	/// <param name="timestamp">
	/// The timestamp supplied by the alternate export.
	/// </param>
	/// <param name="sender">
	/// The sender supplied by the alternate export.
	/// </param>
	[TestCase("2026-11-18T01:16:34", "Jobs <jobs@example.com>")]
	[TestCase("2026-11-17T01:16:34", "Jobs <other@example.com>")]
	[TestCase("2026-11-17T01:16:34", "Other <jobs@example.com>")]
	[TestCase("2026-11-17T01:16:34Z", "Jobs <jobs@example.com>")]
	public void ReadDirectory_ReportsConflictingMetadata(
		string timestamp, string sender)
	{
		WriteFile(
			"message.txt",
			"Sent: " + timestamp + "\nSender: " + sender + "\n\nListing");
		string htmlExport = "<!-- Sent: 2026-11-17T01:16:34\n" +
			"Sender: Jobs &lt;jobs@example.com&gt; -->\n\n<body>Job</body>";
		WriteFile("message.html", htmlExport);

		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Failed));
		Assert.That(result.Email, Is.Null);
		Assert.That(result.Message, Does.Contain("do not match"));
	}

	/// <summary>
	/// Checks that invalid metadata produces a failed read.
	/// </summary>
	/// <param name="content">The malformed export content.</param>
	[TestCase("Sent: not-a-date\nSender: Jobs <jobs@example.com>")]
	[TestCase("Sent: 2026-11-17T01:16:34\nMissing sender")]
	[TestCase("Sent: 2026-11-17T01:16:34\nSender: invalid-address")]
	[TestCase("Sent: 2026-11-17T01:16:34\nSender:")]
	public void ReadDirectory_ReportsInvalidMetadata(string content)
	{
		WriteFile("message.txt", content);

		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Failed));
		Assert.That(result.Email, Is.Null);
		Assert.That(result.Message, Is.Not.Empty);
	}

	/// <summary>
	/// Checks MIME decoding and precedence over coexisting legacy exports.
	/// </summary>
	[Test]
	public void ReadDirectory_PrefersEmlAndDecodesBothBodies()
	{
		string message =
			"From: =?utf-8?Q?Jos=C3=A9?= <jobs@example.com>\r\n" +
			"Date: Tue, 17 Nov 2026 18:00:00 +0900\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: multipart/alternative; boundary=body\r\n\r\n" +
			"--body\r\n" +
			"Content-Type: text/plain; charset=utf-8\r\n" +
			"Content-Transfer-Encoding: quoted-printable\r\n\r\n" +
			"Caf=C3=A9 jobs\r\n" +
			"--body\r\n" +
			"Content-Type: text/html; charset=utf-8\r\n\r\n" +
			"<p>Job listings</p>\r\n" +
			"--body--\r\n";
		WriteFile("entry-id.EML", message);
		WriteFile("entry-id.txt", "Legacy export without metadata");

		EmailReadResult result = ReadSingle();
		DateTime expected = new DateTime(
			2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Read));
		Assert.That(result.SourceName, Is.EqualTo("entry-id"));
		Assert.That(result.Email!.SentOn, Is.EqualTo(expected));
		Assert.That(result.Email.SentOn.Kind, Is.EqualTo(DateTimeKind.Utc));
		Assert.That(result.Email.SenderName, Is.EqualTo("Jos\u00e9"));
		Assert.That(
			result.Email.SenderAddress, Is.EqualTo("jobs@example.com"));
		Assert.That(result.Email.TextBody, Is.EqualTo("Caf\u00e9 jobs"));
		Assert.That(result.Email.HtmlBody, Is.EqualTo("<p>Job listings</p>"));
	}

	/// <summary>
	/// Checks that a malformed EML is reported as an item failure.
	/// </summary>
	[Test]
	public void ReadDirectory_ReportsMalformedEml()
	{
		WriteFile("malformed.eml", "Unrecognized email export");

		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Failed));
		Assert.That(result.SourceName, Is.EqualTo("malformed"));
		Assert.That(result.Email, Is.Null);
		Assert.That(result.Message, Is.Not.Empty);
	}

	/// <summary>
	/// Checks that unsupported files are reported alongside valid reads.
	/// </summary>
	[Test]
	public void ReadDirectory_IgnoredFilesDoNotPreventValidReads()
	{
		WriteFile("unknown.msg", "Unrecognized email export");
		WriteFile("malformed.txt", "Sent: 2026-11-17T01:16:34\nSender:");
		WriteFile("unknown.html", "<html>No exported metadata</html>");
		WriteFile(
			"valid.txt",
			"Sent: 2026-11-17T01:16:34\nSender: jobs@example.com\n\nListing");
		using FileStream lockedFile = new FileStream(
			Path.Combine(directoryPath, "locked.txt"),
			FileMode.Create,
			FileAccess.ReadWrite,
			FileShare.None);

		IReadOnlyList<EmailReadResult> results =
			ExportedEmailReader.ReadDirectory(directoryPath);

		int readCount = results.Count(
			result => result.Status == EmailReadStatus.Read);
		int ignoredCount = results.Count(
			result => result.Status == EmailReadStatus.Ignored);
		int failedCount = results.Count(
			result => result.Status == EmailReadStatus.Failed);
		bool allReasonsPresent = results
			.Where(result => result.Status != EmailReadStatus.Read)
			.All(result => !string.IsNullOrWhiteSpace(result.Message));
		Assert.That(readCount, Is.EqualTo(1));
		Assert.That(ignoredCount, Is.EqualTo(2));
		Assert.That(failedCount, Is.EqualTo(2));
		Assert.That(allReasonsPresent, Is.True);
	}

	/// <summary>
	/// Checks that an empty body is distinct from an absent body.
	/// </summary>
	[Test]
	public void ReadDirectory_EmptyBodyIsDistinctFromAnAbsentBody()
	{
		WriteFile(
			"message.txt",
			"Sent: 2026-11-17T01:16:34\nSender: jobs@example.com\n\n");

		EmailReadResult result = ReadSingle();

		Assert.That(result.Status, Is.EqualTo(EmailReadStatus.Read));
		Assert.That(result.Email!.TextBody, Is.Empty);
		Assert.That(result.Email.HtmlBody, Is.Null);
	}

	/// <summary>
	/// Checks that an empty directory produces no read outcomes.
	/// </summary>
	[Test]
	public void ReadDirectory_EmptyDirectoryHasNoOutcomes()
	{
		IReadOnlyList<EmailReadResult> results =
			ExportedEmailReader.ReadDirectory(directoryPath);

		Assert.That(results, Is.Empty);
	}

	/// <summary>
	/// Checks that a missing input directory produces an error.
	/// </summary>
	[Test]
	public void ReadDirectory_MissingDirectoryFailsClearly()
	{
		Assert.Throws<DirectoryNotFoundException>(
			() => ExportedEmailReader.ReadDirectory(
				Path.Combine(directoryPath, "missing")));
	}

	private EmailReadResult ReadSingle()
	{
		return ExportedEmailReader.ReadDirectory(directoryPath).Single();
	}

	private void WriteFile(string filename, string content)
	{
		File.WriteAllText(Path.Combine(directoryPath, filename), content);
	}
}
