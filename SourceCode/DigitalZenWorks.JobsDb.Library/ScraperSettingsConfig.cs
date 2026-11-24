/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperSettingsConfig.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// Holds browser and diagnostic preferences for scraper configuration.
/// </summary>
public class ScraperSettingsConfig
{
	/// <summary>
	/// Gets or sets a value indicating whether to request headless browsing.
	/// </summary>
	public bool RunHeadless { get; set; }

	/// <summary>
	/// Gets or sets the configured page load timeout in seconds.
	/// </summary>
	public int PageLoadTimeoutSeconds { get; set; }

	/// <summary>
	/// Gets or sets the configured implicit browser wait in seconds.
	/// </summary>
	public int ImplicitWaitSeconds { get; set; }

	/// <summary>
	/// Gets or sets the configured delay between scrolls in milliseconds.
	/// </summary>
	public int ScrollDelayMs { get; set; }

	/// <summary>
	/// Gets or sets the configured sign-in timeout in seconds.
	/// </summary>
	public int LoginTimeoutSeconds { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to request debug HTML output.
	/// </summary>
	public bool SaveDebugHtml { get; set; }
}
