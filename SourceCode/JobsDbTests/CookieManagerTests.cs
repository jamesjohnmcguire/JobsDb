namespace JobsDbTests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using JobsDb.Core.Data;
	using JobsDb.Core.Repositories;
	using JobsDb.Core.Scrapers;
	using JobsDb.Core.Services;
	using JobsDbLibrary.Scrapers;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	internal class CookieManagerTests
	{
		private CookieManager _cookieManager;
		private string _testCookiesPath;

		[SetUp]
		public void SetUp()
		{
			_cookieManager = new CookieManager();
			_testCookiesPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"JobsDb",
				"Cookies");
		}

		[TearDown]
		public void TearDown()
		{
			// Clean up test cookies
			if (Directory.Exists(_testCookiesPath))
			{
				var testFiles = Directory.GetFiles(_testCookiesPath, "test_*.json");
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
			Assert.DoesNotThrow(() => _cookieManager.ClearCookies("NonExistent"));
		}

		[Test]
		public void HasValidCookies_NonExistentSource_ReturnsFalse()
		{
			// Act
			var result = _cookieManager.HasValidCookies("NonExistent");

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		public void ClearCookies_ExistingCookies_RemovesFile()
		{
			// Arrange
			var testSource = "test_source_" + Guid.NewGuid();
			var cookiePath = Path.Combine(_testCookiesPath, $"{testSource.ToLower()}_cookies.json");
			Directory.CreateDirectory(_testCookiesPath);
			File.WriteAllText(cookiePath, "[]");

			// Act
			_cookieManager.ClearCookies(testSource);

			// Assert
			Assert.That(File.Exists(cookiePath), Is.False);
		}
	}
}
