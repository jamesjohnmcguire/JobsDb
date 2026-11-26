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

/// <summary>
/// Covers dashboard loading, review order, and import failures.
/// </summary>
[TestFixture]
internal sealed class JobsDashboardViewModelTests
{
	/// <summary>
	/// Checks load notifications and the initial posting selection.
	/// </summary>
	/// <returns>A task that completes after verification.</returns>
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
			viewModel.StatusMessage, Is.EqualTo("Inbox: 1 posting."));
		List<string> expectedNotifications = new List<string>
		{
			"Jobs", "SelectedJob", "StatusMessage", "IsLoading", "CanLoad"
		};
		Assert.That(notifications, Is.SupersetOf(expectedNotifications));
	}

	/// <summary>
	/// Checks oldest-first Inbox and application-first Reviewed ordering.
	/// </summary>
	/// <returns>A task that completes after checking the tab orders.</returns>
	[Test]
	public async Task ReviewTabs_UseTheirDefaultOrdering()
	{
		Job oldest = CreatePosting(1, 1);
		Job newest = CreatePosting(2, 8);
		Job unrated = CreatePosting(3, 2, ApplicationStatus.Applied);
		Job interviewing = CreatePosting(
			4, 3, ApplicationStatus.Interviewing, priority: 1);
		Job olderHigh = CreatePosting(5, 1, priority: 5);
		Job newerHigh = CreatePosting(6, 4, priority: 4);
		Job medium = CreatePosting(7, 9, priority: 3);
		Mock<IJobQueryService> queries = new Mock<IJobQueryService>();
		queries.Setup(service => service.GetJobsAsync(false))
			.ReturnsAsync(new List<Job>
			{
				medium, newest, newerHigh, interviewing, unrated,
				olderHigh, oldest
			});
		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(queries.Object);

		await viewModel.LoadJobsAsync().ConfigureAwait(false);

		Job[] expectedInbox = { oldest, newest };
		Assert.That(viewModel.VisibleJobs, Is.EqualTo(expectedInbox));
		viewModel.SelectedReviewTab = 1;
		Job[] expectedReviewed =
		{
			interviewing, unrated, newerHigh, olderHigh, medium
		};
		Assert.That(viewModel.VisibleJobs, Is.EqualTo(expectedReviewed));
	}

	/// <summary>
	/// Checks that a pending load prevents another query.
	/// </summary>
	/// <returns>A task that completes after verification.</returns>
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
			viewModel.StatusMessage, Is.EqualTo("Inbox: 0 postings."));
	}

	/// <summary>
	/// Checks that a failed reload preserves the current jobs and selection.
	/// </summary>
	/// <returns>A task that completes after verification.</returns>
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
			viewModel.StatusMessage, Is.EqualTo("Inbox: 1 posting."));
	}

	/// <summary>
	/// Checks that an import error restores the dashboard's controls.
	/// </summary>
	/// <returns>A task that completes after the failure is reported.</returns>
	[Test]
	public async Task ImportEmails_FailureAllowsRetry()
	{
		Mock<IJobQueryService> queries = new Mock<IJobQueryService>();
		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(queries.Object);

		await viewModel.ImportEmailsAsync(" ").ConfigureAwait(false);

		Assert.That(viewModel.IsLoading, Is.False);
		Assert.That(viewModel.CanLoad, Is.True);
		Assert.That(viewModel.StatusMessage, Does.Contain("Unable to import"));
		queries.VerifyNoOtherCalls();
	}

	private static Job CreatePosting(
		int id,
		int day,
		ApplicationStatus status = ApplicationStatus.NotApplied,
		int? priority = null)
	{
		Job job = new Job();
		job.Id = id;
		job.DatePosted = new DateTime(2026, 11, day, 0, 0, 0, DateTimeKind.Utc);
		job.Status = status;
		job.Priority = priority;
		return job;
	}
}
