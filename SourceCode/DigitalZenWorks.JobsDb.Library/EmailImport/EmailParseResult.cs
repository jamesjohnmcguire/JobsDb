/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailParseResult.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System.Collections.Generic;

/// <summary>
/// A parsing outcome, not a claim that candidates were imported.
/// </summary>
/// <param name="Status">The extraction outcome.</param>
/// <param name="IsAggregate">Whether a digest template was recognized.</param>
/// <param name="Candidates">Distinct posting candidates from this email.</param>
/// <param name="Messages">Reasons for ignoring or warnings about candidates.</param>
public sealed record EmailParseResult(
	EmailParseStatus Status,
	bool IsAggregate,
	IReadOnlyList<EmailJobCandidate> Candidates,
	IReadOnlyList<string> Messages);
