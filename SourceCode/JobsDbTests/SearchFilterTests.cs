/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilterTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using DigitalZenWorks.JobsDb.Library.Scrapers;
using NUnit.Framework;

[TestFixture]
internal class SearchFilterTests
{
	[Test]
	public void SearchFilter_NewInstance_HasDefaultValues()
	{
		// Arrange & Act
		SearchFilter filter = new SearchFilter();

		// Assert
		Assert.That(filter.Id, Is.EqualTo(0));
		Assert.That(filter.IsActive, Is.False);
	}

	[Test]
	public void SearchFilter_SetProperties_ValuesAreSet()
	{
		// Arrange
		SearchFilter filter = new SearchFilter();

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
