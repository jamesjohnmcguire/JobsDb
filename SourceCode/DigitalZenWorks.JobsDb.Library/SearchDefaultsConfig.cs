/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchDefaultsConfig.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// Holds the default criteria supplied for new job searches.
/// </summary>
public class SearchDefaultsConfig
{
	/// <summary>
	/// Gets or sets the default search keywords.
	/// </summary>
	public string Keywords { get; set; }

	/// <summary>
	/// Gets or sets the default search location.
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	/// Gets or sets the default employment type.
	/// </summary>
	public string JobType { get; set; }
}
