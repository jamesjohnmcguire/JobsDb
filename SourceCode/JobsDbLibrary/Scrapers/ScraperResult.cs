/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperResult.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbLibrary.Scrapers;

using System;
using System.Collections.Generic;
using JobsDb.Core.Models;

/// <summary>
/// Result returned by scraper operations
/// </summary>
public class ScraperResult
{
	public TimeSpan Duration { get; set; }

	public string ErrorMessage { get; set; }

	public List<string> Errors { get; set; } = new List<string>();

	public List<Job> Jobs { get; set; } = new List<Job>();

	public int JobsAdded { get; set; }

	public int JobsFound { get; set; }

	public int JobsUpdated { get; set; }

	public int NewJobsCount { get; set; }

	public bool Success { get; set; }

	public int UpdatedJobsCount { get; set; }
}
