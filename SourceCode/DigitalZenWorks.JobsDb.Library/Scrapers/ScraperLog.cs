/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperLog.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Records the outcome of one acquisition run.
/// </summary>
public class ScraperLog
{
	/// <summary>
	/// Gets or sets the database identifier of the log entry.
	/// </summary>
	[Key]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the source that was queried.
	/// </summary>
	[Required]
	public string Source { get; set; }

	/// <summary>
	/// Gets or sets the time the run was logged.
	/// </summary>
	[Required]
	public DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the number of job listings found.
	/// </summary>
	public int JobsFound { get; set; }

	/// <summary>
	/// Gets or sets the number of job listings inserted.
	/// </summary>
	public int JobsAdded { get; set; }

	/// <summary>
	/// Gets or sets the number of job listings updated.
	/// </summary>
	public int JobsUpdated { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the run reported success.
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Gets or sets the failure detail recorded for the run.
	/// </summary>
	public string ErrorMessage { get; set; }

	/// <summary>
	/// Gets or sets the elapsed acquisition time in milliseconds.
	/// </summary>
	public int DurationMs { get; set; }
}
