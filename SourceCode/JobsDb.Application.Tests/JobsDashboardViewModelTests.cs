/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDashboardViewModelTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Application.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Application;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

[TestFixture]
internal sealed class JobsDashboardViewModelTests
{
	[Test]
	public async Task LoadJobsAsync_NotifiesBindingsAndSelectsFirstJob()
	{
		Job job = new Job { Id = 1, Title = "Developer" };
		Mock<IJobQueryService> queries = new Mock<IJobQueryService>();
		queries.Setup(service => service.GetJobsAsync(false))
			.ReturnsAsync(new List<Job> { job });
		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(queries.Object);
		HashSet<string?> notifications = new HashSet<string?>();
		viewModel.PropertyChanged += (sender, arguments) =>
			notifications.Add(arguments.PropertyName);

		await viewModel.LoadJobsAsync().ConfigureAwait(false);

		Assert.That(viewModel.Jobs, Is.EqualTo(new[] { job }));
		Assert.That(viewModel.SelectedJob, Is.SameAs(job));
		Assert.That(
			viewModel.StatusMessage, Is.EqualTo("Loaded 1 active job."));
		List<string> expectedNotifications = new List<string>
		{
			"Jobs", "SelectedJob", "StatusMessage", "IsLoading", "CanLoad"
		};
		Assert.That(notifications, Is.SupersetOf(expectedNotifications));
	}

	[Test]
	public async Task LoadJobsAsync_IgnoresOverlappingRequests()
	{
		TaskCompletionSource<List<Job>> pending =
			new TaskCompletionSource<List<Job>>();
		Mock<IJobQueryService> queries = new Mock<IJobQueryService>();
		queries.Setup(service => service.GetJobsAsync(false))
			.Returns(pending.Task);
		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(queries.Object);

		Task loading = viewModel.LoadJobsAsync();
		Assert.That(viewModel.IsLoading, Is.True);
		Assert.That(viewModel.CanLoad, Is.False);
		Assert.That(viewModel.StatusMessage, Is.EqualTo("Loading jobs..."));
		await viewModel.LoadJobsAsync().ConfigureAwait(false);
		queries.Verify(service => service.GetJobsAsync(false), Times.Once);
		pending.SetResult(new List<Job>());
		await loading.ConfigureAwait(false);

		Assert.That(viewModel.IsLoading, Is.False);
		Assert.That(viewModel.CanLoad, Is.True);
		Assert.That(viewModel.Jobs, Is.Empty);
		Assert.That(viewModel.SelectedJob, Is.Null);
		Assert.That(
			viewModel.StatusMessage, Is.EqualTo("No active jobs found."));
	}

	[Test]
	public async Task LoadJobsAsync_FailureKeepsJobsAndAllowsRetry()
	{
		Job originalJob = new Job { Id = 1, Title = "Original" };
		Job updatedJob = new Job { Id = 1, Title = "Updated" };
		Mock<IJobQueryService> queries = new Mock<IJobQueryService>();
		queries.SetupSequence(service => service.GetJobsAsync(false))
			.ReturnsAsync(new List<Job> { originalJob })
			.ThrowsAsync(new InvalidOperationException("Read failed"))
			.ReturnsAsync(new List<Job> { updatedJob });
		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(queries.Object);
		await viewModel.LoadJobsAsync().ConfigureAwait(false);

		await viewModel.LoadJobsAsync().ConfigureAwait(false);

		Assert.That(viewModel.Jobs, Is.EqualTo(new[] { originalJob }));
		Assert.That(viewModel.SelectedJob, Is.SameAs(originalJob));
		Assert.That(viewModel.StatusMessage, Does.Contain("Read failed"));
		Assert.That(viewModel.CanLoad, Is.True);

		await viewModel.LoadJobsAsync().ConfigureAwait(false);

		Assert.That(viewModel.Jobs, Is.EqualTo(new[] { updatedJob }));
		Assert.That(viewModel.SelectedJob, Is.SameAs(updatedJob));
		Assert.That(
			viewModel.StatusMessage, Is.EqualTo("Loaded 1 active job."));
	}
}
