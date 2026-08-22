/////////////////////////////////////////////////////////////////////////////
// <copyright file="CookieManagerTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Data;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

[TestFixture]
internal class CookieManagerTests
{
	private CookieManager cookieManager;
	private string testCookiesPath;

	[SetUp]
	public void SetUp()
	{
		cookieManager = new CookieManager();
		testCookiesPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"JobsDb",
			"Cookies");
	}

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

	[Test]
	public void ClearCookies_NonExistentSource_DoesNotThrow()
	{
		// Act & Assert
		Assert.DoesNotThrow(() => cookieManager.ClearCookies("NonExistent"));
	}

	[Test]
	public void HasValidCookies_NonExistentSource_ReturnsFalse()
	{
		// Act
		var result = cookieManager.HasValidCookies("NonExistent");

		// Assert
		Assert.That(result, Is.False);
	}

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
