/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilter.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents saved criteria for searching a job source.
/// </summary>
public class SearchFilter
{
	/// <summary>
	/// Gets or sets the database identifier.
	/// </summary>
	[Key]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the display name of the saved filter.
	/// </summary>
	[Required]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the search keywords.
	/// </summary>
	public string Keywords { get; set; }

	/// <summary>
	/// Gets or sets the location to search.
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	/// Gets or sets the source selected for the search.
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the filter is enabled.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Gets or sets the time the filter was created.
	/// </summary>
	public DateTime CreatedDate { get; set; }

	/// <summary>
	/// Gets or sets the last recorded run time, when available.
	/// </summary>
	public DateTime? LastRunDate { get; set; }
}
