/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailParseStatus.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

/// <summary>
/// Describes listing extraction independently of file reading and importing.
/// </summary>
public enum EmailParseStatus
{
	/// <summary>One or more candidates were extracted, possibly with warnings.</summary>
	Parsed,

	/// <summary>The sender, body, or message template is not supported.</summary>
	Ignored,

	/// <summary>Some recognized posting cards could not be read.</summary>
	NeedsReview
}
