/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailJobCandidate.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;

/// <summary>
/// Preliminary source-owned listing data for preview and later import.
/// Missing details remain null until enrichment or review.
/// </summary>
/// <param name="Source">The posting provider.</param>
/// <param name="SourceJobId">The provider's stable posting identifier.</param>
/// <param name="SourceUrl">The canonical posting URL.</param>
/// <param name="Title">The extracted job title.</param>
/// <param name="Company">The company, when provided by the card.</param>
/// <param name="Location">The location, when provided by the card.</param>
/// <param name="DatePosted">The initial posting-date value.</param>
/// <param name="IsPostingDateEstimated">Whether the date is inferred.</param>
public sealed record EmailJobCandidate(
	string Source,
	string SourceJobId,
	Uri SourceUrl,
	string Title,
	string? Company,
	string? Location,
	DateTime DatePosted,
	bool IsPostingDateEstimated);
