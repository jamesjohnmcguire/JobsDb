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
/// Covers EML metadata, MIME decoding, and read outcomes.
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
	/// Checks decoding of message headers and both MIME bodies.
	/// </summary>
	[Test]
	public void ReadDirectory_DecodesBothMimeBodies()
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
	/// Checks that reports and legacy exports are skipped.
	/// </summary>
	[Test]
	public void ReadDirectory_SkipsFilesOtherThanEml()
	{
		WriteFile("processed.txt", "old-id");
		WriteFile("unprocessed.txt", "other-id");
		WriteFile("legacy.txt", "Old text export");
		WriteFile("legacy.html", "<p>Old HTML export</p>");

		IReadOnlyList<EmailReadResult> results =
			ExportedEmailReader.ReadDirectory(directoryPath);

		Assert.That(results, Is.Empty);
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
