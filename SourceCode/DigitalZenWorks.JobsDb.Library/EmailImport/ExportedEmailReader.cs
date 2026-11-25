/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExportedEmailReader.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Reads EML and paired HTML/text exports without modifying their source files.
/// </summary>
public static class ExportedEmailReader
{
	private static readonly string[] TimestampFormats =
	{
		"yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
		"yyyy-MM-dd'T'HH:mm:ssK"
	};

	/// <summary>
	/// Reads immediate directory contents. Missing directories and directory
	/// enumeration failures propagate; item failures are returned individually.
	/// </summary>
	/// <param name="directoryPath">The export directory.</param>
	/// <returns>One outcome per basename or unsupported file.</returns>
	public static IReadOnlyList<EmailReadResult> ReadDirectory(
		string directoryPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
		string[] paths = Directory.GetFiles(directoryPath);
		Array.Sort(paths, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, List<string>> groups =
			new Dictionary<string, List<string>>(
				StringComparer.OrdinalIgnoreCase);
		List<EmailReadResult> results = new List<EmailReadResult>();

		foreach (string path in paths)
		{
			string extension = Path.GetExtension(path);
			if (!IsEml(extension) && !IsHtml(extension) &&
				!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
			{
				results.Add(new EmailReadResult(
					Path.GetFileName(path),
					EmailReadStatus.Ignored,
					null,
					"Unsupported file extension."));
				continue;
			}

			string sourceName = Path.GetFileNameWithoutExtension(path);
			if (!groups.TryGetValue(sourceName, out List<string>? group))
			{
				group = new List<string>();
				groups.Add(sourceName, group);
			}

			group.Add(path);
		}

		foreach (KeyValuePair<string, List<string>> group in groups)
		{
			results.Add(ReadExport(group.Key, group.Value));
		}

		return results.OrderBy(
			result => result.SourceName, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	private static Email CreateEmail(
		DateTime sentOn,
		MailAddress? sender,
		string htmlBody,
		string textBody,
		bool isHtml)
	{
		Email email = new Email();
		email.SentOn = sentOn;
		email.SenderName = sender.DisplayName;
		email.SenderAddress = sender.Address;

		if (isHtml)
		{
			email.HtmlBody = htmlBody;
		}
		else
		{
			email.TextBody = textBody;
		}

		return email;
	}

	private static MailAddress? CreateSender(string senderLine)
	{
		MailAddress? sender = null;

		string senderText = senderLine.Substring(7);
		senderText = senderText.Trim();

		if (!MailAddress.TryCreate(senderText, out sender))
		{
			throw new FormatException("Invalid Sender export address.");
		}

		return sender;
	}

	private static DateTime GetSentTime(string sentLine)
	{
		string timestamp = sentLine.Substring(5);
		timestamp = timestamp.Trim();

		bool result = DateTime.TryParseExact(
			timestamp,
			TimestampFormats,
			CultureInfo.InvariantCulture,
			DateTimeStyles.RoundtripKind,
			out DateTime sentOn);

		if (result == false)
		{
			throw new FormatException("Invalid Sent export timestamp.");
		}

		if (sentOn.Kind == DateTimeKind.Local)
		{
			sentOn = sentOn.ToUniversalTime();
		}

		return sentOn;
	}

	private static bool IsEml(string extension)
	{
		return extension.Equals(".eml", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsHtml(string extension)
	{
		return extension.Equals(".html", StringComparison.OrdinalIgnoreCase);
	}

	private static EmailReadResult ReadExport(
		string sourceName, List<string> paths)
	{
		List<string> emlPaths = paths.FindAll(
			path => IsEml(Path.GetExtension(path)));
		if (emlPaths.Count == 0)
		{
			return ReadGroup(sourceName, paths);
		}

		if (emlPaths.Count == 1)
		{
			return EmlEmailReader.Read(sourceName, emlPaths[0]);
		}

		return new EmailReadResult(
			sourceName, EmailReadStatus.Failed, null, "Duplicate EML exports.");
	}

	private static EmailReadResult ReadGroup(
		string sourceName, List<string> paths)
	{
		try
		{
			Email? email = null;
			foreach (string path in paths)
			{
				bool isHtml = IsHtml(Path.GetExtension(path));
				string content = File.ReadAllText(path);
				Email? part = ReadPart(content, isHtml);
				if (part == null)
				{
					string message = "Unrecognized export metadata: " +
						Path.GetFileName(path);
					return new EmailReadResult(
						sourceName,
						EmailReadStatus.Ignored,
						null,
						message);
				}

				if (email == null)
				{
					email = part;
					continue;
				}

				if (email.SentOn != part.SentOn ||
					email.SentOn.Kind != part.SentOn.Kind ||
					!email.SenderAddress.Equals(
						part.SenderAddress,
						StringComparison.OrdinalIgnoreCase) ||
					!email.SenderName.Equals(
						part.SenderName, StringComparison.Ordinal))
				{
					throw new FormatException(
						"HTML and text export metadata do not match.");
				}

				if (isHtml)
				{
					if (email.HtmlBody != null)
					{
						throw new FormatException("Duplicate HTML export.");
					}

					email.HtmlBody = part.HtmlBody;
				}
				else
				{
					if (email.TextBody != null)
					{
						throw new FormatException("Duplicate text export.");
					}

					email.TextBody = part.TextBody;
				}
			}

			return new EmailReadResult(
				sourceName, EmailReadStatus.Read, email, null);
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException or
				FormatException)
		{
			return new EmailReadResult(
				sourceName, EmailReadStatus.Failed, null, exception.Message);
		}
	}

	private static Email? ReadPart(string content, bool isHtml)
	{
		string metadata = content;
		string body = string.Empty;
		if (isHtml)
		{
			string trimmed = content.TrimStart();
			if (!trimmed.StartsWith("<!--", StringComparison.Ordinal))
			{
				return null;
			}

			int end = trimmed.IndexOf("-->", StringComparison.Ordinal);
			if (end < 0)
			{
				return null;
			}

			metadata = WebUtility.HtmlDecode(trimmed.Substring(4, end - 4));
			body = RemoveLineBreak(trimmed.Substring(end + 3));
			body = RemoveLineBreak(body);
		}

		metadata = metadata.TrimStart();
		using StringReader reader = new StringReader(metadata);

		string? sentLine = reader.ReadLine();

		if (sentLine == null ||
			!sentLine.StartsWith("Sent:", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		string? senderLine = reader.ReadLine();

		if (senderLine == null || !senderLine.StartsWith(
			"Sender:", StringComparison.OrdinalIgnoreCase))
		{
			throw new FormatException("Missing Sender export metadata.");
		}

		DateTime sentOn = GetSentTime(sentLine);

		MailAddress? sender = CreateSender(senderLine);

		string textBody = string.Empty;
		if (isHtml == false)
		{
			string text = reader.ReadToEnd();
			textBody = RemoveLineBreak(text);
		}

		Email email = CreateEmail(
			sentOn,
			sender,
			body,
			textBody,
			isHtml);

		return email;
	}

	private static string RemoveLineBreak(string text)
	{
		if (text.StartsWith("\r\n", StringComparison.Ordinal))
		{
			text = text.Substring(2);
		}

		if (text.StartsWith('\n'))
		{
			text = text.Substring(1);
		}

		return text;
	}
}
