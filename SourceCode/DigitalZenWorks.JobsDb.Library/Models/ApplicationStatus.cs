/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationStatus.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Describes the user's application progress for a job.
/// </summary>
public enum ApplicationStatus
{
	/// <summary>
	/// The user has yet to apply.
	/// </summary>
	NotApplied,

	/// <summary>
	/// The user submitted an application.
	/// </summary>
	Applied,

	/// <summary>
	/// The user is in the interview process.
	/// </summary>
	Interviewing,

	/// <summary>
	/// The employer made an offer.
	/// </summary>
	Offered,

	/// <summary>
	/// The employer declined the application.
	/// </summary>
	Rejected,

	/// <summary>
	/// The user withdrew the application.
	/// </summary>
	Withdrawn,

	/// <summary>
	/// The user accepted the offer.
	/// </summary>
	Accepted
}
