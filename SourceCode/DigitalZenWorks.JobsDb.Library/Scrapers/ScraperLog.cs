/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperLog.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbLibrary.Scrapers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ScraperLog
{
	[Key]
	public int Id { get; set; }

	[Required]
	public string Source { get; set; }

	[Required]
	public DateTime Timestamp { get; set; }

	public int JobsFound { get; set; }

	public int JobsAdded { get; set; }

	public int JobsUpdated { get; set; }

	public bool Success { get; set; }

	public string ErrorMessage { get; set; }

	public int DurationMs { get; set; }
}
