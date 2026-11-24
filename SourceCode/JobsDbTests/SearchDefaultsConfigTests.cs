/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchDefaultsConfigTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using DigitalZenWorks.JobsDb.Library;
using NUnit.Framework;

/// <summary>
/// Covers the configurable default search criteria.
/// </summary>
[TestFixture]
internal sealed class SearchDefaultsConfigTests
{
	/// <summary>
	/// Checks that the model retains the assigned search defaults.
	/// </summary>
	[Test]
	public void SearchDefaultsConfig_SetProperties_ValuesAreSet()
	{
		// Arrange & Act
		SearchDefaultsConfig config = new SearchDefaultsConfig
		{
			Keywords = "software engineer",
			Location = "Tokyo, Japan",
			JobType = "Full-time"
		};

		// Assert
		Assert.That(config.Keywords, Is.EqualTo("software engineer"));
		Assert.That(config.Location, Is.EqualTo("Tokyo, Japan"));
		Assert.That(config.JobType, Is.EqualTo("Full-time"));
	}
}
