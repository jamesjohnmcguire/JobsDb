/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperResult.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.Collections.Generic;
using DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Result returned by scraper operations.
/// </summary>
public class ScraperResult
{
	/// <summary>
	/// Gets or sets the elapsed time of the acquisition run.
	/// </summary>
	public TimeSpan Duration { get; set; }

	/// <summary>
	/// Gets or sets the overall failure message for the run.
	/// </summary>
	public string ErrorMessage { get; set; }

	/// <summary>
	/// Gets or sets the individual problems reported during acquisition.
	/// </summary>
	public List<string> Errors { get; set; } = new List<string>();

	/// <summary>
	/// Gets or sets the listings returned by the scraper.
	/// </summary>
	public List<Job> Jobs { get; set; } = new List<Job>();

	/// <summary>
	/// Gets or sets the number of listings inserted.
	/// </summary>
	public int JobsAdded { get; set; }

	/// <summary>
	/// Gets or sets the number of listings found.
	/// </summary>
	public int JobsFound { get; set; }

	/// <summary>
	/// Gets or sets the number of listings updated.
	/// </summary>
	public int JobsUpdated { get; set; }

	/// <summary>
	/// Gets or sets the scraper's count of new listings.
	/// </summary>
	public int NewJobsCount { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the run reported success.
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Gets or sets the scraper's count of existing listings updated.
	/// </summary>
	public int UpdatedJobsCount { get; set; }
}
