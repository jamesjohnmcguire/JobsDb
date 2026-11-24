/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobQueryServiceTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

/// <summary>
/// Covers the query service's collection selection and error propagation.
/// </summary>
[TestFixture]
internal sealed class JobQueryServiceTests
{
	/// <summary>
	/// Checks that the service chooses the requested reader operation.
	/// </summary>
	/// <param name="includeArchived">
	/// Whether the query requests archived listings.
	/// </param>
	/// <returns>A task that completes when the assertions finish.</returns>
	[TestCase(false)]
	[TestCase(true)]
	public async Task GetJobsAsync_SelectsRequestedCollection(
		bool includeArchived)
	{
		Job activeJob = new Job { Title = "Active", IsArchived = false };
		Job archivedJob = new Job { Title = "Archived", IsArchived = true };
		List<Job> activeJobs = new List<Job> { activeJob };
		List<Job> allJobs = new List<Job> { activeJob, archivedJob };
		Mock<IJobReader> repository = new Mock<IJobReader>(
			MockBehavior.Strict);
		repository.Setup(instance => instance.GetActiveJobsAsync())
			.ReturnsAsync(activeJobs);
		repository.Setup(instance => instance.GetAllAsync())
			.ReturnsAsync(allJobs);
		IJobQueryService service = new JobQueryService(repository.Object);

		List<Job> jobs =
			await service.GetJobsAsync(includeArchived).ConfigureAwait(false);

		Assert.That(jobs, Contains.Item(activeJob));
		if (includeArchived)
		{
			Assert.That(jobs, Contains.Item(archivedJob));
		}
		else
		{
			Assert.That(jobs, Does.Not.Contain(archivedJob));
		}
	}

	/// <summary>
	/// Checks that the service propagates the reader's original exception.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetJobsAsync_FailedRead_DoesNotReturnAnEmptyCollection()
	{
		InvalidOperationException failure =
			new InvalidOperationException("Database unavailable");
		Mock<IJobReader> repository = new Mock<IJobReader>();
		repository.Setup(instance => instance.GetActiveJobsAsync())
			.ThrowsAsync(failure);
		IJobQueryService service = new JobQueryService(repository.Object);

		InvalidOperationException? exception =
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => service.GetJobsAsync())
				.ConfigureAwait(false);

		Assert.That(exception, Is.SameAs(failure));
	}
}
