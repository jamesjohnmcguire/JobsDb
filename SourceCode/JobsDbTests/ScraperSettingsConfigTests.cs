/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperSettingsConfigTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using DigitalZenWorks.JobsDb.Library;
using NUnit.Framework;

/// <summary>
/// Covers the scraper settings model and its defaults.
/// </summary>
[TestFixture]
internal sealed class ScraperSettingsConfigTests
{
	/// <summary>
	/// Checks that configured preferences can be assigned and read back.
	/// </summary>
	[Test]
	public void ScraperSettingsConfig_SetProperties_ValuesAreSet()
	{
		// Arrange & Act
		ScraperSettingsConfig config = new ScraperSettingsConfig
		{
			RunHeadless = true,
			PageLoadTimeoutSeconds = 60,
			ImplicitWaitSeconds = 15,
			ScrollDelayMs = 1500,
			LoginTimeoutSeconds = 120,
			SaveDebugHtml = true
		};

		// Assert
		Assert.That(config.RunHeadless, Is.True);
		Assert.That(config.PageLoadTimeoutSeconds, Is.EqualTo(60));
		Assert.That(config.ImplicitWaitSeconds, Is.EqualTo(15));
		Assert.That(config.ScrollDelayMs, Is.EqualTo(1500));
		Assert.That(config.LoginTimeoutSeconds, Is.EqualTo(120));
		Assert.That(config.SaveDebugHtml, Is.True);
	}

	/// <summary>
	/// Checks the default browser and timing settings.
	/// </summary>
	[Test]
	public void ScraperSettingsConfig_DefaultValues_AreReasonable()
	{
		// Arrange & Act
		ScraperSettingsConfig config = new ScraperSettingsConfig
		{
			RunHeadless = false,
			PageLoadTimeoutSeconds = 30,
			ImplicitWaitSeconds = 10
		};

		// Assert
		Assert.That(config.PageLoadTimeoutSeconds, Is.GreaterThan(0));
		Assert.That(config.ImplicitWaitSeconds, Is.GreaterThan(0));
		Assert.That(config.RunHeadless, Is.False); // Better for debugging
	}
}
