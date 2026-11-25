/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmlEmailReader.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.IO;
using System.Linq;
using DigitalZenWorks.JobsDb.Library.Models;
using MimeKit;
using MimeKit.Utils;

/// <summary>
/// Reads MIME email exports without modifying the source.
/// </summary>
internal static class EmlEmailReader
{
	/// <summary>
	/// Reads the message metadata and available HTML and plain-text bodies.
	/// </summary>
	/// <param name="sourceName">The exporter file basename.</param>
	/// <param name="path">The EML file path.</param>
	/// <returns>The decoded message or an item-level failure.</returns>
	public static EmailReadResult Read(string sourceName, string path)
	{
		try
		{
			using FileStream stream = File.OpenRead(path);
			MimeMessage message = MimeMessage.Load(stream);
			Email email = DecodeMessage(message);
			return new EmailReadResult(
				sourceName, EmailReadStatus.Read, email, null);
		}
		catch (FormatException)
		{
			return new EmailReadResult(
				sourceName,
				EmailReadStatus.Failed,
				null,
				"Invalid EML message or missing date or sender.");
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException)
		{
			return new EmailReadResult(
				sourceName, EmailReadStatus.Failed, null, exception.Message);
		}
	}

	private static Email DecodeMessage(MimeMessage message)
	{
		string? date = message.Headers[HeaderId.Date];
		if (date == null ||
			!DateUtils.TryParse(date, out DateTimeOffset sentOn))
		{
			throw new FormatException();
		}

		MailboxAddress? sender = message.From.Mailboxes.FirstOrDefault();
		if (sender == null)
		{
			sender = message.Sender;
		}

		if (sender == null || string.IsNullOrWhiteSpace(sender.Address))
		{
			throw new FormatException();
		}

		Email email = new Email();
		email.SentOn = sentOn.UtcDateTime;
		email.SenderName = sender.Name ?? string.Empty;
		email.SenderAddress = sender.Address;
		email.HtmlBody = message.HtmlBody;
		email.TextBody = message.TextBody;
		return email;
	}
}
