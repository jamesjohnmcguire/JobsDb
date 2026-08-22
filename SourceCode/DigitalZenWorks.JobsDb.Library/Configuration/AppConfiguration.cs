/////////////////////////////////////////////////////////////////////////////
// <copyright file="AppConfiguration.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Configuration;

using System.Collections.Generic;

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

	public string? MasterPassword { get; set; }

	public List<CredentialConfig>? Credentials { get; set; }

	public SearchDefaultsConfig? SearchDefaults { get; set; }

	public ScraperSettingsConfig? ScraperSettings { get; set; }
}
