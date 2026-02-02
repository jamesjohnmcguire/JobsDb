/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperSettingsConfig.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Configuration;

public class ScraperSettingsConfig
{
	public bool RunHeadless { get; set; }
	public int PageLoadTimeoutSeconds { get; set; }
	public int ImplicitWaitSeconds { get; set; }
	public int ScrollDelayMs { get; set; }
	public int LoginTimeoutSeconds { get; set; }
	public bool SaveDebugHtml { get; set; }
}
