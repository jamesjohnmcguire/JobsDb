/////////////////////////////////////////////////////////////////////////////
// <copyright file="PostingReviewTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Application.Tests;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using DigitalZenWorks.JobsDb.Application;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Services;
using Moq;
using NUnit.Framework;

/// <summary>
/// Covers posting links and the selected job description.
/// </summary>
[TestFixture]
internal sealed class PostingReviewTests
{
	/// <summary>
	/// Checks that the selected web address opens in the browser.
	/// </summary>
	/// <param name="address">The selected posting URL.</param>
	[TestCase("https://example.com/jobs/123")]
	[TestCase("http://example.com/jobs/123")]
	public void OpenSelectedPosting_UsesSelectedWebAddress(string address)
	{
		List<Uri> opened = new List<Uri>();
		JobsDashboardViewModel viewModel = CreateViewModel(opened.Add);
		viewModel.SelectedJob = new Job { SourceUrl = address };

		viewModel.OpenSelectedPosting();

		Assert.That(viewModel.CanOpenPosting, Is.True);
		Assert.That(opened, Has.Count.EqualTo(1));
		Assert.That(opened[0].AbsoluteUri, Is.EqualTo(address));
		Assert.That(viewModel.StatusMessage, Does.Contain("Opening"));
	}

	/// <summary>
	/// Checks that unusable posting addresses never reach the browser.
	/// </summary>
	/// <param name="address">The invalid or disallowed posting address.</param>
	[TestCase("")]
	[TestCase("not an address")]
	[TestCase("/jobs/123")]
	[TestCase("file:///C:/Windows/notepad.exe")]
	[TestCase("javascript:alert(1)")]
	[TestCase("mailto:someone@example.com")]
	[TestCase("https://user@example.com/jobs/123")]
	public void OpenSelectedPosting_RejectsNonWebOrCredentialLinks(
		string address)
	{
		List<Uri> opened = new List<Uri>();
		JobsDashboardViewModel viewModel = CreateViewModel(opened.Add);
		viewModel.SelectedJob = new Job { SourceUrl = address };

		viewModel.OpenSelectedPosting();

		Assert.That(viewModel.CanOpenPosting, Is.False);
		Assert.That(opened, Is.Empty);
		Assert.That(viewModel.StatusMessage, Does.Contain("HTTP or HTTPS"));
	}

	/// <summary>
	/// Checks that opening a posting requires a selection.
	/// </summary>
	[Test]
	public void OpenSelectedPosting_NoSelectionDoesNotLaunch()
	{
		List<Uri> opened = new List<Uri>();
		JobsDashboardViewModel viewModel = CreateViewModel(opened.Add);

		viewModel.OpenSelectedPosting();

		Assert.That(viewModel.CanOpenPosting, Is.False);
		Assert.That(opened, Is.Empty);
		Assert.That(
			viewModel.SelectedDescription, Does.Contain("Select a job"));
	}

	/// <summary>
	/// Checks that the status line reports a browser launch failure.
	/// </summary>
	[Test]
	public void OpenSelectedPosting_BrowserFailureIsDisplayed()
	{
		JobsDashboardViewModel viewModel = CreateViewModel(
			address => throw new Win32Exception("No browser available"));
		viewModel.SelectedJob = new Job
		{
			SourceUrl = "https://example.com/jobs/123"
		};

		viewModel.OpenSelectedPosting();

		Assert.That(
			viewModel.StatusMessage, Does.Contain("No browser available"));
	}

	/// <summary>
	/// Checks that selection changes update link and description bindings.
	/// </summary>
	[Test]
	public void SelectionChangesNotifyPostingAndDescriptionBindings()
	{
		JobsDashboardViewModel viewModel = CreateViewModel(address => { });
		HashSet<string?> notifications = new HashSet<string?>();
		viewModel.PropertyChanged += (sender, arguments) =>
			notifications.Add(arguments.PropertyName);

		viewModel.SelectedJob = new Job
		{
			SourceUrl = "https://example.com/jobs/123",
			Description = "A saved full description."
		};

		Assert.That(notifications, Does.Contain("CanOpenPosting"));
		Assert.That(notifications, Does.Contain("SelectedDescription"));
		Assert.That(
			viewModel.SelectedDescription,
			Is.EqualTo("A saved full description."));

		viewModel.SelectedJob = null;

		Assert.That(viewModel.CanOpenPosting, Is.False);
		Assert.That(
			viewModel.SelectedDescription, Does.Contain("Select a job"));
	}

	/// <summary>
	/// Checks the placeholder for an absent job description.
	/// </summary>
	/// <param name="description">The absent or blank description.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase(" \n ")]
	public void Description_ExplainsMissingEnrichment(string? description)
	{
		JobsDashboardViewModel viewModel = CreateViewModel(address => { });
		viewModel.SelectedJob = new Job { Description = description };

		Assert.That(
			viewModel.SelectedDescription,
			Does.Contain("No description saved yet"));
	}

	private static JobsDashboardViewModel CreateViewModel(
		Action<Uri> openPosting)
	{
		return new JobsDashboardViewModel(
			Mock.Of<IJobQueryService>(), openPosting);
	}
}
