/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationStatusTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Models;

using System;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using NUnit.Framework;

[TestFixture]
internal class ApplicationStatusTests
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
