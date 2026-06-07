/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperCredentialTests.cs" company="Digital Zen Works">
// Copyright ｩ 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Models;

using NUnit.Framework;
using JobsDb.Core.Models;
using System;
using JobsDbLibrary.Scrapers;
using JobsDb.Core.Scrapers;

[TestFixture]
public class ScraperCredentialTests
{
	[Test]
	public void ScraperCredential_NewInstance_IsActive()
	{
		// Arrange & Act
		var credential = new ScraperCredential
		{
			Source = "LinkedIn",
			Username = "test@example.com",
			EncryptedPassword = "encrypted_password",
			IsActive = true
		};

		// Assert
		Assert.That(credential.Source, Is.EqualTo("LinkedIn"));
		Assert.That(credential.Username, Is.EqualTo("test@example.com"));
		Assert.That(credential.IsActive, Is.True);
	}

	[Test]
	public void ScraperCredential_SetLastUsed_DateIsSet()
	{
		// Arrange
		var credential = new ScraperCredential();
		var lastUsed = DateTime.UtcNow;

		// Act
		credential.LastUsed = lastUsed;

		// Assert
		Assert.That(credential.LastUsed, Is.Not.Null);
		Assert.That(credential.LastUsed.Value, Is.EqualTo(lastUsed).Within(TimeSpan.FromSeconds(1)));
	}
}
