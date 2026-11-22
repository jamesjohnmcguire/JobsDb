/////////////////////////////////////////////////////////////////////////////
// <copyright file="IEmailJobParser.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Extracts listing candidates without network access or database writes.
/// </summary>
public interface IEmailJobParser
{
	/// <summary>
	/// Recognizes a supported message template and extracts its listings.
	/// </summary>
	/// <param name="email">The source email, which is not modified.</param>
	/// <returns>Candidates, classification, and diagnostic messages.</returns>
	EmailParseResult Parse(Email email);
}
