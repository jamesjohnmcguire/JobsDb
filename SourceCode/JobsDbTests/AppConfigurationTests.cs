/////////////////////////////////////////////////////////////////////////////
// <copyright file="AppConfigurationTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System.Text.Json;
using DigitalZenWorks.JobsDb.Library;
using NUnit.Framework;

/// <summary>
/// Covers the JSON representation of application settings.
/// </summary>
[TestFixture]
internal class AppConfigurationTests
{
	/// <summary>
	/// Checks that settings can be assigned on a new configuration.
	/// </summary>
	[Test]
	public void AppConfiguration_NewInstance_CanSetProperties()
	{
		// Arrange & Act
		AppConfiguration config = new AppConfiguration
		{
			MasterPassword = "TestPassword",
			Credentials = new System.Collections.Generic.List<CredentialConfig>(),
			SearchDefaults = new SearchDefaultsConfig(),
			ScraperSettings = new ScraperSettingsConfig()
		};

		// Assert
		Assert.That(config.MasterPassword, Is.EqualTo("TestPassword"));
		Assert.That(config.Credentials, Is.Not.Null);
		Assert.That(config.SearchDefaults, Is.Not.Null);
		Assert.That(config.ScraperSettings, Is.Not.Null);
	}

	/// <summary>
	/// Checks that serialization emits the expected JSON settings.
	/// </summary>
	[Test]
	public void AppConfiguration_SerializeToJson_ProducesValidJson()
	{
		// Arrange
		AppConfiguration config = new AppConfiguration
		{
			MasterPassword = "TestPass",
			Credentials = new System.Collections.Generic.List<CredentialConfig>
		{
			new CredentialConfig
			{
				Source = "LinkedIn",
				Username = "test@example.com",
				Password = "pass",
				IsActive = true
			}
		},
			SearchDefaults = new SearchDefaultsConfig
			{
				Keywords = "software developer",
				Location = "Tokyo",
				JobType = "Full-time"
			},
			ScraperSettings = new ScraperSettingsConfig
			{
				RunHeadless = false,
				PageLoadTimeoutSeconds = 30
			}
		};

		// Act
		var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

		// Assert
		Assert.That(json, Is.Not.Null);
		Assert.That(json, Does.Contain("TestPass"));
		Assert.That(json, Does.Contain("LinkedIn"));
		Assert.That(json, Does.Contain("software developer"));
	}

	/// <summary>
	/// Checks that JSON input reconstructs the expected settings.
	/// </summary>
	[Test]
	public void AppConfiguration_DeserializeFromJson_CreatesValidObject()
	{
		// Arrange
		var json = @"{
			""MasterPassword"": ""TestPass"",
			""Credentials"": [
				{
					""Source"": ""LinkedIn"",
					""Username"": ""test@example.com"",
					""Password"": ""pass"",
					""IsActive"": true
				}
			],
			""SearchDefaults"": {
				""Keywords"": ""developer"",
				""Location"": ""Tokyo"",
				""JobType"": ""Full-time""
			},
			""ScraperSettings"": {
				""RunHeadless"": false,
				""PageLoadTimeoutSeconds"": 30
			}
		}";

		// Act
		var config = JsonSerializer.Deserialize<AppConfiguration>(json);

		// Assert
		Assert.That(config, Is.Not.Null);
		Assert.That(config.MasterPassword, Is.EqualTo("TestPass"));
		Assert.That(config.Credentials.Count, Is.EqualTo(1));
		Assert.That(config.Credentials[0].Source, Is.EqualTo("LinkedIn"));
		Assert.That(config.SearchDefaults.Keywords, Is.EqualTo("developer"));
		Assert.That(config.ScraperSettings.RunHeadless, Is.False);
	}
}
