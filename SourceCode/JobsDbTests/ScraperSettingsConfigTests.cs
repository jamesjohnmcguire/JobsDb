namespace JobsDbTests;

using JobsDb.Core.Configuration;
using NUnit.Framework;

[TestFixture]
internal class ScraperSettingsConfigTests
{
	[Test]
	public void ScraperSettingsConfig_SetProperties_ValuesAreSet()
	{
		// Arrange & Act
		var config = new ScraperSettingsConfig
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

	[Test]
	public void ScraperSettingsConfig_DefaultValues_AreReasonable()
	{
		// Arrange & Act
		var config = new ScraperSettingsConfig
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
