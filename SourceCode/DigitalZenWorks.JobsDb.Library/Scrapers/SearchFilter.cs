/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilter.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.ComponentModel.DataAnnotations;

public class SearchFilter
{
	[Key]
	public int Id { get; set; }

	[Required]
	public string Name { get; set; }

	public string Keywords { get; set; }

	public string Location { get; set; }

	public string Source { get; set; }

	public bool IsActive { get; set; }

	public DateTime CreatedDate { get; set; }

	public DateTime? LastRunDate { get; set; }
}
