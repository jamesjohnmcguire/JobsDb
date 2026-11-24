/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using DigitalZenWorks.JobsDb.Library.Models;
using NUnit.Framework;

/// <summary>
/// Covers the job model's default values and application fields.
/// </summary>
[TestFixture]
internal sealed class JobTests
{
	/// <summary>
	/// Checks the initial application and archive state of a new job.
	/// </summary>
	[Test]
	public void Job_NewInstance_HasDefaultValues()
	{
		// Arrange & Act
		Job job = new Job();

		// Assert
		Assert.That(job.Id, Is.EqualTo(0));
		Assert.That(job.Status, Is.EqualTo(ApplicationStatus.NotApplied));
		Assert.That(job.IsArchived, Is.False);
	}

	/// <summary>
	/// Checks that the model retains assigned listing and application fields.
	/// </summary>
	[Test]
	public void Job_SetProperties_ValuesAreSet()
	{
		// Arrange
		Job job = new Job();
		var testDate = DateTime.UtcNow;

		// Act
		job.Title = "Senior Software Engineer";
		job.Company = "Tech Corp";
		job.Location = "Tokyo, Japan";
		job.Source = "LinkedIn";
		job.SourceUrl = "https://linkedin.com/jobs/123";
		job.SourceJobId = "123456";
		job.DatePosted = testDate;
		job.Status = ApplicationStatus.Applied;
		job.Priority = 5;

		// Assert
		Assert.That(job.Title, Is.EqualTo("Senior Software Engineer"));
		Assert.That(job.Company, Is.EqualTo("Tech Corp"));
		Assert.That(job.Location, Is.EqualTo("Tokyo, Japan"));
		Assert.That(job.Source, Is.EqualTo("LinkedIn"));
		Assert.That(job.SourceUrl, Is.EqualTo("https://linkedin.com/jobs/123"));
		Assert.That(job.SourceJobId, Is.EqualTo("123456"));
		Assert.That(job.DatePosted, Is.EqualTo(testDate));
		Assert.That(job.Status, Is.EqualTo(ApplicationStatus.Applied));
		Assert.That(job.Priority, Is.EqualTo(5));
	}

	/// <summary>
	/// Checks that the salary bounds and currency can be set.
	/// </summary>
	[Test]
	public void Job_SetSalaryRange_ValuesAreSet()
	{
		// Arrange
		Job job = new Job();

		// Act
		job.SalaryMin = 5000000;
		job.SalaryMax = 8000000;
		job.SalaryCurrency = "JPY";

		// Assert
		Assert.That(job.SalaryMin, Is.EqualTo(5000000));
		Assert.That(job.SalaryMax, Is.EqualTo(8000000));
		Assert.That(job.SalaryCurrency, Is.EqualTo("JPY"));
	}

	/// <summary>
	/// Checks that the application date can be recorded on an applied job.
	/// </summary>
	[Test]
	public void Job_MarkAsApplied_DateAppliedIsSet()
	{
		// Arrange
		Job job = new Job();
		var appliedDate = DateTime.UtcNow;

		// Act
		job.Status = ApplicationStatus.Applied;
		job.DateApplied = appliedDate;

		// Assert
		Assert.That(job.Status, Is.EqualTo(ApplicationStatus.Applied));
		Assert.That(job.DateApplied, Is.Not.Null);
		Assert.That(job.DateApplied.Value, Is.EqualTo(appliedDate).Within(TimeSpan.FromSeconds(1)));
	}

	/// <summary>
	/// Checks that a job can be marked archived.
	/// </summary>
	[Test]
	public void Job_Archive_IsArchivedIsTrue()
	{
		// Arrange
		Job job = new Job { IsArchived = false };

		// Act
		job.IsArchived = true;

		// Assert
		Assert.That(job.IsArchived, Is.True);
	}
}
