/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailJobParserRouter.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Selects the job parser registered for an email's sender.
/// </summary>
internal sealed class EmailJobParserRouter : IEmailJobParser
{
	private readonly Dictionary<string, IEmailJobParser> parsersBySender;

	/// <summary>
	/// Initializes a new instance of the <see cref="EmailJobParserRouter"/>
	/// class.
	/// </summary>
	/// <param name="parsersBySender">Parsers by sender address.</param>
	internal EmailJobParserRouter(
		IReadOnlyDictionary<string, IEmailJobParser> parsersBySender)
	{
		this.parsersBySender = new Dictionary<string, IEmailJobParser>(
			parsersBySender, StringComparer.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public EmailParseResult Parse(Email email)
	{
		ArgumentNullException.ThrowIfNull(email);
		if (parsersBySender.TryGetValue(
			email.SenderAddress, out IEmailJobParser? parser))
		{
			return parser.Parse(email);
		}

		return new EmailParseResult(
			EmailParseStatus.Ignored,
			false,
			Array.Empty<EmailJobCandidate>(),
			new[] { "No job parser is registered for this sender." });
	}
}
