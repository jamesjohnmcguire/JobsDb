/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperResultTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using JobsDb.Core.Models;
using JobsDbLibrary.Scrapers;
using NUnit.Framework;

[TestFixture]
internal class ScraperResultTests
{
	[Test]
	public void ScraperResult_NewInstance_HasDefaultValues()
	{
		// Act
		var result = new ScraperResult();

		// Assert
		Assert.That(result.Success, Is.False);
		Assert.That(result.JobsFound, Is.EqualTo(0));
		Assert.That(result.JobsAdded, Is.EqualTo(0));
		Assert.That(result.JobsUpdated, Is.EqualTo(0));
		Assert.That(result.ErrorMessage, Is.Null);
		Assert.That(result.Jobs, Is.Not.Null);
		Assert.That(result.Jobs.Count, Is.EqualTo(0));
	}

	[Test]
	public void ScraperResult_SetProperties_ValuesAreSet()
	{
		// Arrange
		var result = new ScraperResult();
		var duration = TimeSpan.FromSeconds(30);

		// Act
		result.Success = true;
		result.JobsFound = 10;
		result.JobsAdded = 5;
		result.JobsUpdated = 3;
		result.Duration = duration;
		result.Jobs.Add(new Job { Title = "Test" });

		// Assert
		Assert.That(result.Success, Is.True);
		Assert.That(result.JobsFound, Is.EqualTo(10));
		Assert.That(result.JobsAdded, Is.EqualTo(5));
		Assert.That(result.JobsUpdated, Is.EqualTo(3));
		Assert.That(result.Duration, Is.EqualTo(duration));
		Assert.That(result.Jobs.Count, Is.EqualTo(1));
	}
}
