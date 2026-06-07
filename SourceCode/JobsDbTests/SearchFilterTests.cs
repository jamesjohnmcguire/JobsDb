/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilterTests.cs" company="Digital Zen Works">
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
