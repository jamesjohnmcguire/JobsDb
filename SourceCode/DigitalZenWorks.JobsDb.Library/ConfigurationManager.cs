/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Manages configuration including credentials loaded from JSON file.
/// </summary>
public class ConfigurationManager
{
	private const string DefaultConfigFileName = "jobsdb_config.json";

	private static readonly JsonSerializerOptions SerializationOptions =
		new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

	private readonly string configurationPath;
	private AppConfiguration configuration = new AppConfiguration();

	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigurationManager"/>
	/// class.
	/// </summary>
	/// <param name="configPath">
	/// The JSON file path, or null to use the default application path.
	/// </param>
	public ConfigurationManager(string? configPath = null)
	{
		configurationPath = ResolveConfigurationPath(configPath);
		LoadConfiguration();
	}

	/// <summary>
	/// Gets the loaded settings or defaults when loading failed.
	/// </summary>
	public AppConfiguration Config => configuration;

	/// <summary>
	/// Writes the current settings to the configured JSON file.
	/// </summary>
	public void SaveConfiguration()
	{
		try
		{
			string json = JsonSerializer.Serialize(
				configuration, SerializationOptions);
			File.WriteAllText(configurationPath, json);
			Console.WriteLine(
				$"Configuration saved to: {configurationPath}");
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException)
		{
			Console.WriteLine(
				$"Error saving configuration: {exception.Message}");
		}
	}

	/// <summary>
	/// Reads the plaintext password supplied by the local JSON configuration.
	/// </summary>
	/// <param name="source">The source whose credential is requested.</param>
	/// <returns>
	/// The configured password, or null when no password is configured.
	/// </returns>
	public string? GetDecryptedPassword(string source)
	{
		CredentialConfig? credential = GetCredentialConfig(source);
		if (string.IsNullOrEmpty(credential?.Password))
		{
			return null;
		}

		// JSON passwords are plaintext; database storage encrypts them.
		return credential.Password;
	}

	/// <summary>
	/// Looks up credential settings by source name.
	/// </summary>
	/// <param name="source">The source whose credential is requested.</param>
	/// <returns>The matching settings, or null if none exist.</returns>
	public CredentialConfig? GetCredentialConfig(string source)
	{
		return configuration.Credentials?.Find(
			credential => credential.Source == source);
	}

	private static string ResolveConfigurationPath(string? configPath)
	{
		if (!string.IsNullOrEmpty(configPath))
		{
			return configPath;
		}

		string localApplicationData = Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData);
		string applicationDataFolder = Path.Combine(
			localApplicationData, "JobsDb");
		Directory.CreateDirectory(applicationDataFolder);
		return Path.Combine(applicationDataFolder, DefaultConfigFileName);
	}

	private void LoadConfiguration()
	{
		if (!File.Exists(configurationPath))
		{
			Console.WriteLine(
				$"Creating default configuration at: {configurationPath}");
			SaveConfiguration();
			return;
		}

		LoadExistingConfiguration();
	}

	private void LoadExistingConfiguration()
	{
		try
		{
			string json = File.ReadAllText(configurationPath);
			AppConfiguration? loadedConfiguration =
				JsonSerializer.Deserialize<AppConfiguration>(json);
			configuration = loadedConfiguration ?? new AppConfiguration();
			Console.WriteLine(
				$"Configuration loaded from: {configurationPath}");
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException or
				JsonException)
		{
			Console.WriteLine(
				$"Error loading configuration: {exception.Message}");
		}
	}
}
