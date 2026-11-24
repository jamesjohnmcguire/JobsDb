/////////////////////////////////////////////////////////////////////////////
// <copyright file="CookieManagerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library.Services;
using NUnit.Framework;

/// <summary>
/// Covers file-based cookie checks and deletion.
/// </summary>
[TestFixture]
internal class CookieManagerTests
{
	private CookieManager cookieManager;
	private string testCookiesPath;

	/// <summary>
	/// Creates the cookie manager and locates its storage directory.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		cookieManager = new CookieManager();
		testCookiesPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"JobsDb",
			"Cookies");
	}

	/// <summary>
	/// Removes cookie files written by the tests.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		// Clean up test cookies
		if (Directory.Exists(testCookiesPath))
		{
			var testFiles = Directory.GetFiles(testCookiesPath, "test_*.json");
			foreach (var file in testFiles)
			{
				try
				{
					File.Delete(file);
				}
				catch
				{
				}
			}
		}
	}

	/// <summary>
	/// Checks that clearing an unknown source does not throw.
	/// </summary>
	[Test]
	public void ClearCookies_NonExistentSource_DoesNotThrow()
	{
		// Act & Assert
		Assert.DoesNotThrow(() => cookieManager.ClearCookies("NonExistent"));
	}

	/// <summary>
	/// Checks that a missing cookie file is reported as unavailable.
	/// </summary>
	[Test]
	public void HasValidCookies_NonExistentSource_ReturnsFalse()
	{
		// Act
		var result = cookieManager.HasValidCookies("NonExistent");

		// Assert
		Assert.That(result, Is.False);
	}

	/// <summary>
	/// Checks that clearing a known source removes its cookie file.
	/// </summary>
	[Test]
	public void ClearCookies_ExistingCookies_RemovesFile()
	{
		// Arrange
		var testSource = "test_source_" + Guid.NewGuid();
		var cookiePath = Path.Combine(testCookiesPath, $"{testSource.ToLower()}_cookies.json");
		Directory.CreateDirectory(testCookiesPath);
		File.WriteAllText(cookiePath, "[]");

		// Act
		cookieManager.ClearCookies(testSource);

		// Assert
		Assert.That(File.Exists(cookiePath), Is.False);
	}
}
