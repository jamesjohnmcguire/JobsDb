/////////////////////////////////////////////////////////////////////////////
// <copyright file="CookieManager.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenQA.Selenium;

/// <summary>
/// Manages browser cookies to persist login sessions
/// Saves cookies after successful login, loads them to avoid re-login
/// </summary>
public class CookieManager
{
	private readonly string _cookiesFolder;

	public CookieManager()
	{
		_cookiesFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"JobsDb",
			"Cookies");

		Directory.CreateDirectory(_cookiesFolder);
	}

	/// <summary>
	/// Save all cookies from the current browser session
	/// </summary>
	public void SaveCookies(IWebDriver driver, string source)
	{
		try
		{
			var cookies = driver.Manage().Cookies.AllCookies;
			List<CookieData> cookieList = cookies.Select(c => new CookieData
			{
				Name = c.Name,
				Value = c.Value,
				Domain = c.Domain,
				Path = c.Path,
				Expiry = c.Expiry,
				IsSecure = c.Secure,
				IsHttpOnly = c.IsHttpOnly,
				SameSite = c.SameSite
			}).ToList();

			var cookiePath = GetCookiePath(source);
			var json = JsonSerializer.Serialize(cookieList, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			File.WriteAllText(cookiePath, json);
			Console.WriteLine($"✓ Saved {cookieList.Count} cookies for {source}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠ Error saving cookies for {source}: {ex.Message}");
		}
	}

	/// <summary>
	/// Load previously saved cookies into the browser
	/// </summary>
	public bool LoadCookies(IWebDriver driver, string source)
	{
		try
		{
			var cookiePath = GetCookiePath(source);

			if (!File.Exists(cookiePath))
			{
				Console.WriteLine($"ℹ No saved cookies found for {source}");
				return false;
			}

			// Check if cookies are still valid (not expired)
			var fileAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(cookiePath);
			if (fileAge.TotalDays > 7) // Cookies older than 7 days are considered stale
			{
				Console.WriteLine($"ℹ Cookies for {source} are too old ({fileAge.TotalDays:F1} days), will re-login");
				File.Delete(cookiePath);
				return false;
			}

			var json = File.ReadAllText(cookiePath);
			var cookieList = JsonSerializer.Deserialize<List<CookieData>>(json);

			if (cookieList == null || !cookieList.Any())
			{
				Console.WriteLine($"⚠ No cookies found in file for {source}");
				return false;
			}

			// Navigate to the domain first (required for cookie setting)
			var firstCookie = cookieList.First();
			var baseUrl = $"https://{firstCookie.Domain.TrimStart('.')}";
			driver.Navigate().GoToUrl(baseUrl);

			int loadedCount = 0;
			foreach (var cookieData in cookieList)
			{
				try
				{
					// Skip expired cookies
					if (cookieData.Expiry.HasValue && cookieData.Expiry.Value < DateTime.UtcNow)
						continue;

					Cookie cookie = new Cookie(
						cookieData.Name,
						cookieData.Value,
						cookieData.Domain,
						cookieData.Path,
						cookieData.Expiry,
						cookieData.IsSecure,
						cookieData.IsHttpOnly,
						cookieData.SameSite);

					driver.Manage().Cookies.AddCookie(cookie);
					loadedCount++;
				}
				catch
				{
					// Some cookies might fail to load, continue with others
				}
			}

			Console.WriteLine($"✓ Loaded {loadedCount} cookies for {source}");
			return loadedCount > 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠ Error loading cookies for {source}: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Delete saved cookies for a source (forces re-login)
	/// </summary>
	public void ClearCookies(string source)
	{
		try
		{
			var cookiePath = GetCookiePath(source);
			if (File.Exists(cookiePath))
			{
				File.Delete(cookiePath);
				Console.WriteLine($"✓ Cleared cookies for {source}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠ Error clearing cookies for {source}: {ex.Message}");
		}
	}

	/// <summary>
	/// Check if we have valid saved cookies
	/// </summary>
	public bool HasValidCookies(string source)
	{
		var cookiePath = GetCookiePath(source);
		if (!File.Exists(cookiePath))
			return false;

		var fileAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(cookiePath);
		return fileAge.TotalDays <= 7;
	}

	private string GetCookiePath(string source)
	{
		return Path.Combine(_cookiesFolder, $"{source.ToLower()}_cookies.json");
	}

	/// <summary>
	/// Internal class to serialize/deserialize cookie data
	/// </summary>
	private class CookieData
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public string Domain { get; set; }

		public string Path { get; set; }

		public DateTime? Expiry { get; set; }

		public bool IsSecure { get; set; }

		public bool IsHttpOnly { get; set; }

		public string SameSite { get; set; }
	}
}
