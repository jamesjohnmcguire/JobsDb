/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationManagerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DigitalZenWorks.JobsDb.Library;
using NUnit.Framework;

[TestFixture]
internal class ConfigurationManagerTests
{
	private string testConfigPath;

	[SetUp]
	public void SetUp()
	{
		testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
	}

	[TearDown]
	public void TearDown()
	{
		if (File.Exists(testConfigPath))
		{
			File.Delete(testConfigPath);
		}
	}

	[Test]
	public void Constructor_NoConfigFile_CreatesDefaultConfiguration()
	{
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);

		Assert.That(manager.Config, Is.Not.Null);
		Assert.That(manager.Config.Credentials, Is.Not.Null);
		Assert.That(File.Exists(testConfigPath), Is.True);
	}

	[Test]
	public void Constructor_ExistingConfigFile_LoadsConfiguration()
	{
		// Arrange
		AppConfiguration config = new AppConfiguration
		{
			MasterPassword = "CustomPassword",
			Credentials = new System.Collections.Generic.List<CredentialConfig>
			{
				new CredentialConfig
				{
					Source = "LinkedIn",
					Username = "test@example.com",
					Password = "testpass",
					IsActive = true
				}
			}
		};

		var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(testConfigPath, json);

		// Act
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);

		// Assert
		Assert.That(manager.Config.MasterPassword, Is.EqualTo("CustomPassword"));
		Assert.That(manager.Config.Credentials[0].Username, Is.EqualTo("test@example.com"));
	}

	[Test]
	public void SaveConfiguration_ValidConfig_SavesToFile()
	{
		// Arrange
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);
		manager.Config.MasterPassword = "NewPassword";

		// Act
		manager.SaveConfiguration();

		// Assert
		Assert.That(File.Exists(testConfigPath), Is.True);

		var json = File.ReadAllText(testConfigPath);
		var loaded = JsonSerializer.Deserialize<AppConfiguration>(json);
		Assert.That(loaded.MasterPassword, Is.EqualTo("NewPassword"));
	}

	[Test]
	public void GetDecryptedPassword_ExistingSource_ReturnsPassword()
	{
		// Arrange
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);
		manager.Config.Credentials.Add(new CredentialConfig
		{
			Source = "TestSource",
			Username = "test@example.com",
			Password = "MyPassword123",
			IsActive = true
		});

		// Act
		var password = manager.GetDecryptedPassword("TestSource");

		// Assert
		Assert.That(password, Is.EqualTo("MyPassword123"));
	}

	[Test]
	public void GetDecryptedPassword_NonExistingSource_ReturnsNull()
	{
		// Arrange
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);

		// Act
		var password = manager.GetDecryptedPassword("NonExistent");

		// Assert
		Assert.That(password, Is.Null);
	}

	[Test]
	public void GetCredentialConfig_ExistingSource_ReturnsConfig()
	{
		ConfigurationManager manager =
			new ConfigurationManager(testConfigPath);
		AppConfiguration appConfig = manager.Config;
		List<CredentialConfig> credentialsList = appConfig.Credentials;

		CredentialConfig credentials = new CredentialConfig
		{
			Source = "LinkedIn",
			Username = "test@linkedin.com",
			Password = "pass123",
			IsActive = true
		};

		credentialsList.Add(credentials);

		CredentialConfig credConfig = manager.GetCredentialConfig("LinkedIn");

		// Assert
		Assert.That(credConfig, Is.Not.Null);
		Assert.That(credConfig.Source, Is.EqualTo("LinkedIn"));
		Assert.That(credConfig.Username, Is.EqualTo("test@linkedin.com"));
	}

	[Test]
	public void GetCredentialConfig_NonExistingSource_ReturnsNull()
	{
		// Arrange
		ConfigurationManager manager = new ConfigurationManager(testConfigPath);

		// Act
		var credConfig = manager.GetCredentialConfig("NonExistent");

		// Assert
		Assert.That(credConfig, Is.Null);
	}
}
