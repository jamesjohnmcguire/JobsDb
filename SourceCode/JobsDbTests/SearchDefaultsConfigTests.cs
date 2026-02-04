/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchDefaultsConfigTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using JobsDb.Core.Configuration;
using NUnit.Framework;

[TestFixture]
internal class SearchDefaultsConfigTests
{
	[Test]
	public void SearchDefaultsConfig_SetProperties_ValuesAreSet()
	{
		// Arrange & Act
		var config = new SearchDefaultsConfig
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
