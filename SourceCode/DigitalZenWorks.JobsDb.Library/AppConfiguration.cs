/////////////////////////////////////////////////////////////////////////////
// <copyright file="AppConfiguration.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System.Collections.Generic;

/// <summary>
/// Holds the JSON settings for JobsDb.
/// </summary>
public class AppConfiguration
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AppConfiguration"/> class.
	/// </summary>
	public AppConfiguration()
	{
		Credentials = new List<CredentialConfig>();
		SearchDefaults = new SearchDefaultsConfig();
		ScraperSettings = new ScraperSettingsConfig();
	}

	/// <summary>
	/// Gets or sets the password configured for credential encryption.
	/// </summary>
	public string? MasterPassword { get; set; }

	/// <summary>
	/// Gets or sets the per-source credential settings.
	/// </summary>
	public List<CredentialConfig>? Credentials { get; set; }

	/// <summary>
	/// Gets or sets the default search criteria.
	/// </summary>
	public SearchDefaultsConfig? SearchDefaults { get; set; }

	/// <summary>
	/// Gets or sets the scraper settings.
	/// </summary>
	public ScraperSettingsConfig? ScraperSettings { get; set; }
}
