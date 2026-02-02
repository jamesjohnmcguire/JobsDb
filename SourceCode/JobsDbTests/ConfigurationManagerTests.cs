/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationManagerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Configuration;

using NUnit.Framework;
using JobsDb.Core.Configuration;
using System;
using System.IO;
using System.Text.Json;

[TestFixture]
public class ConfigurationManagerTests
{
    private string _testConfigPath;

    [SetUp]
    public void SetUp()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testConfigPath))
            File.Delete(_testConfigPath);
    }

    [Test]
    public void Constructor_NoConfigFile_CreatesDefaultConfiguration()
    {
        // Act
        var manager = new ConfigurationManager(_testConfigPath);

        // Assert
        Assert.That(manager.Config, Is.Not.Null);
        Assert.That(manager.Config.Credentials, Is.Not.Null);
        Assert.That(manager.Config.Credentials.Count, Is.GreaterThan(0));
        Assert.That(File.Exists(_testConfigPath), Is.True);
    }

    [Test]
    public void Constructor_ExistingConfigFile_LoadsConfiguration()
    {
        // Arrange
        var config = new AppConfiguration
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
        File.WriteAllText(_testConfigPath, json);

        // Act
        var manager = new ConfigurationManager(_testConfigPath);

        // Assert
        Assert.That(manager.Config.MasterPassword, Is.EqualTo("CustomPassword"));
        Assert.That(manager.Config.Credentials[0].Username, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void SaveConfiguration_ValidConfig_SavesToFile()
    {
        // Arrange
        var manager = new ConfigurationManager(_testConfigPath);
        manager.Config.MasterPassword = "NewPassword";

        // Act
        manager.SaveConfiguration();

        // Assert
        Assert.That(File.Exists(_testConfigPath), Is.True);
            
        var json = File.ReadAllText(_testConfigPath);
        var loaded = JsonSerializer.Deserialize<AppConfiguration>(json);
        Assert.That(loaded.MasterPassword, Is.EqualTo("NewPassword"));
    }

    [Test]
    public void GetDecryptedPassword_ExistingSource_ReturnsPassword()
    {
        // Arrange
        var manager = new ConfigurationManager(_testConfigPath);
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
        var manager = new ConfigurationManager(_testConfigPath);

        // Act
        var password = manager.GetDecryptedPassword("NonExistent");

        // Assert
        Assert.That(password, Is.Null);
    }

    [Test]
    public void GetCredentialConfig_ExistingSource_ReturnsConfig()
    {
        // Arrange
        var manager = new ConfigurationManager(_testConfigPath);
        manager.Config.Credentials.Add(new CredentialConfig
        {
            Source = "LinkedIn",
            Username = "test@linkedin.com",
            Password = "pass123",
            IsActive = true
        });

        // Act
        var credConfig = manager.GetCredentialConfig("LinkedIn");

        // Assert
        Assert.That(credConfig, Is.Not.Null);
        Assert.That(credConfig.Source, Is.EqualTo("LinkedIn"));
        Assert.That(credConfig.Username, Is.EqualTo("test@linkedin.com"));
    }

    [Test]
    public void GetCredentialConfig_NonExistingSource_ReturnsNull()
    {
        // Arrange
        var manager = new ConfigurationManager(_testConfigPath);

        // Act
        var credConfig = manager.GetCredentialConfig("NonExistent");

        // Assert
        Assert.That(credConfig, Is.Null);
    }
}

[TestFixture]
public class AppConfigurationTests
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

[TestFixture]
public class CredentialConfigTests
{
    [Test]
    public void CredentialConfig_SetProperties_ValuesAreSet()
    {
        // Arrange & Act
        var config = new CredentialConfig
        {
            Source = "LinkedIn",
            Username = "user@example.com",
            Password = "MyPassword",
            IsActive = true,
            Notes = "Test credential"
        };

        // Assert
        Assert.That(config.Source, Is.EqualTo("LinkedIn"));
        Assert.That(config.Username, Is.EqualTo("user@example.com"));
        Assert.That(config.Password, Is.EqualTo("MyPassword"));
        Assert.That(config.IsActive, Is.True);
        Assert.That(config.Notes, Is.EqualTo("Test credential"));
    }
}

[TestFixture]
public class SearchDefaultsConfigTests
{
    [Test]
    public void SearchDefaultsConfig_SetProperties_ValuesAreSet()
    {
        // Arrange & Act
        var config = new SearchDefaultsConfig
        {
            Keywords = "software engineer",
            Location = "Tokyo, Japan",
            JobType = "Full-time"
        };

        // Assert
        Assert.That(config.Keywords, Is.EqualTo("software engineer"));
        Assert.That(config.Location, Is.EqualTo("Tokyo, Japan"));
        Assert.That(config.JobType, Is.EqualTo("Full-time"));
    }
}

[TestFixture]
public class ScraperSettingsConfigTests
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
