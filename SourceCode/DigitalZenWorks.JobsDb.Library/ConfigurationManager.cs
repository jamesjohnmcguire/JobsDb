/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Manages configuration including credentials loaded from JSON file.
/// </summary>
public class ConfigurationManager
{
	private readonly string _configPath;
	private AppConfiguration config;
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

	public AppConfiguration Config => config;

	private void LoadConfiguration()
	{
		if (File.Exists(_configPath))
		{
			try
			{
				var json = File.ReadAllText(_configPath);
				config = JsonSerializer.Deserialize<AppConfiguration>(json);
				Console.WriteLine($"✓ Configuration loaded from: {_configPath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠ Error loading config: {ex.Message}");
				config = CreateDefaultConfiguration();
			}
		}
		else
		{
			Console.WriteLine($"ℹ No config file found. Creating default at: {_configPath}");
			config = CreateDefaultConfiguration();
			SaveConfiguration();
		}
	}

	public void SaveConfiguration()
	{
		try
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			var json = JsonSerializer.Serialize(config, options);
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
		AppConfiguration appConfig = new AppConfiguration();

		return appConfig;
	}

	public string GetDecryptedPassword(string source)
	{
		var credential = config.Credentials.Find(c => c.Source == source);
		if (credential == null || string.IsNullOrEmpty(credential.Password))
		{
			return null;
		}

		// Password is stored in plain text in JSON for simplicity
		// It gets encrypted when stored in the database
		return credential.Password;
	}

	public CredentialConfig GetCredentialConfig(string source)
	{
		CredentialConfig? credentialsConfig =
			config.Credentials.Find(c => c.Source == source);
		return credentialsConfig;
	}
}
