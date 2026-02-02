namespace JobsDbLibrary.Scrapers;

using JobsDb.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
