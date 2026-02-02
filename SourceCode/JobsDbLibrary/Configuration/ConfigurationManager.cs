namespace JobsDb.Core.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobsDb.Core.Models;
//	using JobsDb.Core.Services;
using JobsDb.Core.Scrapers;

/// <summary>
/// Manages configuration including credentials loaded from JSON file
/// </summary>
public class ConfigurationManager
{
	private readonly string _configPath;
	private AppConfiguration _config;
	private const string DefaultConfigFileName = "jobsdb_config.json";

	public ConfigurationManager(string configPath = null)
	{
		if (string.IsNullOrEmpty(configPath))
		{
			var appDataFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"JobsDb");
			Directory.CreateDirectory(appDataFolder);
			_configPath = Path.Combine(appDataFolder, DefaultConfigFileName);
		}
		else
		{
			_configPath = configPath;
		}

		LoadConfiguration();
	}

	public AppConfiguration Config => _config;

	private void LoadConfiguration()
	{
		if (File.Exists(_configPath))
		{
			try
			{
				var json = File.ReadAllText(_configPath);
				_config = JsonSerializer.Deserialize<AppConfiguration>(json);
				Console.WriteLine($"✓ Configuration loaded from: {_configPath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠ Error loading config: {ex.Message}");
				_config = CreateDefaultConfiguration();
			}
		}
		else
		{
			Console.WriteLine($"ℹ No config file found. Creating default at: {_configPath}");
			_config = CreateDefaultConfiguration();
			SaveConfiguration();
		}
	}

	public void SaveConfiguration()
	{
		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			var json = JsonSerializer.Serialize(_config, options);
			File.WriteAllText(_configPath, json);
			Console.WriteLine($"✓ Configuration saved to: {_configPath}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ Error saving config: {ex.Message}");
		}
	}

	private AppConfiguration CreateDefaultConfiguration()
	{
		return new AppConfiguration
		{
			MasterPassword = "JobsDb2024!",
			Credentials = new List<CredentialConfig>
			{
				new CredentialConfig
				{
					Source = "LinkedIn",
					Username = "your_email@example.com",
					Password = "your_password_here",
					IsActive = true
				},
				new CredentialConfig
				{
					Source = "TokyoDev",
					Username = "your_email@example.com",
					Password = "your_password_here",
					IsActive = false // TokyoDev doesn't require login currently
				}
			},
			SearchDefaults = new SearchDefaultsConfig
			{
				Keywords = "software developer",
				Location = "Tokyo, Japan",
				JobType = "Full-time"
			},
			ScraperSettings = new ScraperSettingsConfig
			{
				RunHeadless = false,
				PageLoadTimeoutSeconds = 30,
				ImplicitWaitSeconds = 10,
				ScrollDelayMs = 2000,
				LoginTimeoutSeconds = 60
			}
		};
	}

	public string GetDecryptedPassword(string source)
	{
		var credential = _config.Credentials.Find(c => c.Source == source);
		if (credential == null || string.IsNullOrEmpty(credential.Password))
			return null;

		// Password is stored in plain text in JSON for simplicity
		// It gets encrypted when stored in the database
		return credential.Password;
	}

	public CredentialConfig GetCredentialConfig(string source)
	{
		return _config.Credentials.Find(c => c.Source == source);
	}
}
