/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationStatusTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using DigitalZenWorks.JobsDb.Library.Models;
using NUnit.Framework;

/// <summary>
/// Covers the application's status values and their names.
/// </summary>
[TestFixture]
internal sealed class ApplicationStatusTests
{
	/// <summary>
	/// Checks the complete set of application status values.
	/// </summary>
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

	/// <summary>
	/// Checks that status values use their expected string names.
	/// </summary>
	[Test]
	public void ApplicationStatus_ConvertToString_ReturnsCorrectValue()
	{
		// Act & Assert
		Assert.That(ApplicationStatus.NotApplied.ToString(), Is.EqualTo("NotApplied"));
		Assert.That(ApplicationStatus.Applied.ToString(), Is.EqualTo("Applied"));
		Assert.That(ApplicationStatus.Interviewing.ToString(), Is.EqualTo("Interviewing"));
	}
}
