/////////////////////////////////////////////////////////////////////////////
// <copyright file="AppConfigurationTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System.Text.Json;
using JobsDb.Core.Configuration;
using NUnit.Framework;

[TestFixture]
internal class AppConfigurationTests
{
	[Test]
	public void AppConfiguration_NewInstance_CanSetProperties()
	{
		// Arrange & Act
		var config = new AppConfiguration
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

	[Test]
	public void AppConfiguration_SerializeToJson_ProducesValidJson()
	{
		// Arrange
		var config = new AppConfiguration
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
