/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportResult.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

/// <summary>
/// A per-message import outcome for interactive callers and command-line logs.
/// </summary>
/// <param name="SourceName">The exported message identifier.</param>
/// <param name="Status">The processing outcome.</param>
/// <param name="Added">New jobs saved.</param>
/// <param name="Updated">Existing job summaries changed.</param>
/// <param name="Unchanged">Existing jobs requiring no changes.</param>
/// <param name="Message">Diagnostics and information requiring review.</param>
public sealed record EmailImportResult(
	string SourceName,
	EmailImportStatus Status,
	int Added,
	int Updated,
	int Unchanged,
	string? Message);
