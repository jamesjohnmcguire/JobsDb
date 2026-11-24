/////////////////////////////////////////////////////////////////////////////
// <copyright file="DashboardThemePreference.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.IO;

/// <summary>
/// Stores the dashboard theme as a user preference.
/// </summary>
internal static class DashboardThemePreference
{
	/// <summary>
	/// Finds the theme preference file in the user's roaming profile.
	/// </summary>
	/// <returns>The full path of the theme preference file.</returns>
	public static string GetPath()
	{
		string roamingData = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData);
		return Path.Combine(roamingData, "JobsDb", "theme.txt");
	}

	/// <summary>
	/// Loads a saved theme. Dark is the default for missing or invalid files.
	/// </summary>
	/// <param name="path">The file path, or null for the default theme.</param>
	/// <returns>The theme name to select.</returns>
	public static string Load(string? path)
	{
		if (!File.Exists(path))
		{
			return "Dark";
		}

		try
		{
			string theme = File.ReadAllText(path).Trim();
			if (theme is "System" or "Light" or "Dark")
			{
				return theme;
			}
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException)
		{
			return "Dark";
		}

		return "Dark";
	}

	/// <summary>
	/// Saves the theme for the next launch.
	/// </summary>
	/// <param name="path">The full path to the preference file.</param>
	/// <param name="theme">The selected theme name.</param>
	public static void Save(string path, string theme)
	{
		FileInfo file = new FileInfo(path);
		file.Directory?.Create();
		File.WriteAllText(file.FullName, theme);
	}
}
