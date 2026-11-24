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

/// <summary>
/// Covers local JSON configuration loading, saving, and credential lookup.
/// </summary>
[TestFixture]
internal sealed class ConfigurationManagerTests
{
	private string testConfigurationPath = string.Empty;

	/// <summary>
	/// Chooses a temporary configuration path for the test.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		testConfigurationPath = Path.Combine(
			Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
	}

	/// <summary>
	/// Removes the configuration file created by the test.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		if (File.Exists(testConfigurationPath))
		{
			File.Delete(testConfigurationPath);
		}
	}

	/// <summary>
	/// Checks that a missing file is initialized with default settings.
	/// </summary>
	[Test]
	public void Constructor_NoConfigFile_CreatesDefaultConfiguration()
	{
		ConfigurationManager manager = CreateManager();

		Assert.That(manager.Config, Is.Not.Null);
		Assert.That(manager.Config.Credentials, Is.Not.Null);
		Assert.That(File.Exists(testConfigurationPath), Is.True);
	}

	/// <summary>
	/// Checks that an existing file supplies settings and credentials.
	/// </summary>
	[Test]
	public void Constructor_ExistingConfigFile_LoadsConfiguration()
	{
		AppConfiguration configuration = new AppConfiguration();
		configuration.MasterPassword = "CustomPassword";
		configuration.Credentials = CreateCredentials();
		string json = JsonSerializer.Serialize(configuration);
		File.WriteAllText(testConfigurationPath, json);

		ConfigurationManager manager = CreateManager();

		Assert.That(
			manager.Config.MasterPassword, Is.EqualTo("CustomPassword"));
		Assert.That(
			manager.Config.Credentials?[0].Username,
			Is.EqualTo("test@example.com"));
	}

	/// <summary>
	/// Checks that changed settings are written to disk.
	/// </summary>
	[Test]
	public void SaveConfiguration_ValidConfig_SavesToFile()
	{
		ConfigurationManager manager = CreateManager();
		manager.Config.MasterPassword = "NewPassword";

		manager.SaveConfiguration();

		string json = File.ReadAllText(testConfigurationPath);
		AppConfiguration? loadedConfiguration =
			JsonSerializer.Deserialize<AppConfiguration>(json);
		Assert.That(
			loadedConfiguration?.MasterPassword, Is.EqualTo("NewPassword"));
	}

	/// <summary>
	/// Checks that a configured source password can be read.
	/// </summary>
	[Test]
	public void GetDecryptedPassword_ExistingSource_ReturnsPassword()
	{
		ConfigurationManager manager = CreateManager();
		manager.Config.Credentials = CreateCredentials();

		string? password = manager.GetDecryptedPassword("TestSource");

		Assert.That(password, Is.EqualTo("MyPassword123"));
	}

	/// <summary>
	/// Checks that a missing source yields no password.
	/// </summary>
	[Test]
	public void GetDecryptedPassword_NonExistingSource_ReturnsNull()
	{
		ConfigurationManager manager = CreateManager();

		string? password = manager.GetDecryptedPassword("NonExistent");

		Assert.That(password, Is.Null);
	}

	/// <summary>
	/// Checks that lookup returns the credential for a configured source.
	/// </summary>
	[Test]
	public void GetCredentialConfig_ExistingSource_ReturnsConfig()
	{
		ConfigurationManager manager = CreateManager();
		manager.Config.Credentials = CreateCredentials();

		CredentialConfig? credential =
			manager.GetCredentialConfig("TestSource");

		Assert.That(credential, Is.Not.Null);
		Assert.That(credential?.Source, Is.EqualTo("TestSource"));
		Assert.That(credential?.Username, Is.EqualTo("test@example.com"));
	}

	/// <summary>
	/// Checks that a missing source yields no credential.
	/// </summary>
	[Test]
	public void GetCredentialConfig_NonExistingSource_ReturnsNull()
	{
		ConfigurationManager manager = CreateManager();

		CredentialConfig? credential =
			manager.GetCredentialConfig("NonExistent");

		Assert.That(credential, Is.Null);
	}

	/// <summary>
	/// Checks that invalid JSON leaves defaults in memory and input intact.
	/// </summary>
	/// <param name="json">The invalid or null JSON to load.</param>
	[TestCase("null")]
	[TestCase("{")]
	[TestCase("[]")]
	public void Constructor_InvalidOrNullJson_KeepsDefaultsAndInput(
		string json)
	{
		File.WriteAllText(testConfigurationPath, json);

		ConfigurationManager manager = CreateManager();

		Assert.That(manager.Config, Is.Not.Null);
		Assert.That(manager.Config.Credentials, Is.Empty);
		Assert.That(
			File.ReadAllText(testConfigurationPath), Is.EqualTo(json));
	}

	/// <summary>
	/// Checks that lookups tolerate omitted or null credential collections.
	/// </summary>
	/// <param name="json">The JSON input with no credential collection.</param>
	[TestCase("{}")]
	[TestCase("{\"Credentials\":null}")]
	public void CredentialLookups_MissingCollection_ReturnNull(string json)
	{
		File.WriteAllText(testConfigurationPath, json);
		ConfigurationManager manager = CreateManager();

		Assert.That(manager.GetCredentialConfig("LinkedIn"), Is.Null);
		Assert.That(manager.GetDecryptedPassword("LinkedIn"), Is.Null);
	}

	private static List<CredentialConfig> CreateCredentials()
	{
		CredentialConfig credential = new CredentialConfig();
		credential.Source = "TestSource";
		credential.Username = "test@example.com";
		credential.Password = "MyPassword123";
		credential.IsActive = true;
		List<CredentialConfig> credentials = new List<CredentialConfig>();
		credentials.Add(credential);
		return credentials;
	}

	private ConfigurationManager CreateManager()
	{
		return new ConfigurationManager(testConfigurationPath);
	}
}
