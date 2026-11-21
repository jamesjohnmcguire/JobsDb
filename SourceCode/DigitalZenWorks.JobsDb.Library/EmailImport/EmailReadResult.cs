/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailReadResult.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// A source identifier and outcome suitable for a caller's processing log.
/// </summary>
/// <param name="SourceName">Export basename, or unsupported filename.</param>
/// <param name="Status">The reading outcome.</param>
/// <param name="Email">The email on success; otherwise null.</param>
/// <param name="Message">A diagnostic for an ignored or failed item.</param>
public sealed record EmailReadResult(
	string SourceName,
	EmailReadStatus Status,
	Email? Email,
	string? Message);
