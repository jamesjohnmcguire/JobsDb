/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationStatusTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Models;

using NUnit.Framework;
using JobsDb.Core.Models;
using System;
using JobsDbLibrary.Scrapers;
using JobsDb.Core.Scrapers;

[TestFixture]
public class ApplicationStatusTests
{
	[Test]
	public void ApplicationStatus_AllValuesAreDefined()
	{
		// Assert
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.NotApplied));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Applied));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Interviewing));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Offered));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Rejected));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Withdrawn));
		Assert.That(Enum.IsDefined(typeof(ApplicationStatus), ApplicationStatus.Accepted));
	}

	[Test]
	public void ApplicationStatus_ConvertToString_ReturnsCorrectValue()
	{
		// Act & Assert
		Assert.That(ApplicationStatus.NotApplied.ToString(), Is.EqualTo("NotApplied"));
		Assert.That(ApplicationStatus.Applied.ToString(), Is.EqualTo("Applied"));
		Assert.That(ApplicationStatus.Interviewing.ToString(), Is.EqualTo("Interviewing"));
	}
}

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

[TestFixture]
public class SearchFilterTests
{
	[Test]
	public void SearchFilter_NewInstance_HasDefaultValues()
	{
		// Arrange & Act
		var filter = new SearchFilter();

		// Assert
		Assert.That(filter.Id, Is.EqualTo(0));
		Assert.That(filter.IsActive, Is.False);
	}

	[Test]
	public void SearchFilter_SetProperties_ValuesAreSet()
	{
		// Arrange
		var filter = new SearchFilter();

		// Act
		filter.Name = "Tokyo Developer Jobs";
		filter.Keywords = "software developer";
		filter.Location = "Tokyo, Japan";
		filter.Source = "LinkedIn";
		filter.IsActive = true;

		// Assert
		Assert.That(filter.Name, Is.EqualTo("Tokyo Developer Jobs"));
		Assert.That(filter.Keywords, Is.EqualTo("software developer"));
		Assert.That(filter.Location, Is.EqualTo("Tokyo, Japan"));
		Assert.That(filter.Source, Is.EqualTo("LinkedIn"));
		Assert.That(filter.IsActive, Is.True);
	}
}
