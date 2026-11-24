/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobsDashboardViewModel.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Services;

/// <summary>
/// Holds the job collection and its presentation state.
/// </summary>
internal sealed class JobsDashboardViewModel : INotifyPropertyChanged
{
	private readonly IJobQueryService jobQueries;
	private readonly Action<Uri> openPosting;
	private IReadOnlyList<Job> jobs = Array.Empty<Job>();
	private Job? selectedJob;
	private string statusMessage = "Ready";
	private bool isLoading;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobsDashboardViewModel"/>
	/// class.
	/// </summary>
	/// <param name="jobQueries">The service supplying stored listings.</param>
	/// <param name="openPosting">
	/// The browser action, or null to use the default launcher.
	/// </param>
	public JobsDashboardViewModel(
		IJobQueryService jobQueries, Action<Uri>? openPosting = null)
	{
		ArgumentNullException.ThrowIfNull(jobQueries);
		this.jobQueries = jobQueries;
		this.openPosting = openPosting ?? PostingLauncher.Open;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Gets the active listings displayed by the dashboard.
	/// </summary>
	public IReadOnlyList<Job> Jobs
	{
		get
		{
			return jobs;
		}
	}

	/// <summary>
	/// Gets or sets the listing selected in the dashboard.
	/// </summary>
	public Job? SelectedJob
	{
		get
		{
			return selectedJob;
		}

		set
		{
			selectedJob = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CanOpenPosting));
			OnPropertyChanged(nameof(SelectedDescription));
		}
	}

	/// <summary>
	/// Gets a value indicating whether the selected posting can be opened.
	/// </summary>
	public bool CanOpenPosting
	{
		get
		{
			return GetSelectedPostingAddress() != null;
		}
	}

	/// <summary>
	/// Gets the selected description or an appropriate placeholder.
	/// </summary>
	public string SelectedDescription
	{
		get
		{
			return GetSelectedDescription();
		}
	}

	/// <summary>
	/// Gets the latest status shown in the dashboard.
	/// </summary>
	public string StatusMessage
	{
		get
		{
			return statusMessage;
		}

		private set
		{
			statusMessage = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Gets a value indicating whether a job load is in progress.
	/// </summary>
	public bool IsLoading
	{
		get
		{
			return isLoading;
		}

		private set
		{
			isLoading = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CanLoad));
		}
	}

	/// <summary>
	/// Gets a value indicating whether the dashboard can reload jobs.
	/// </summary>
	public bool CanLoad
	{
		get
		{
			return !isLoading;
		}
	}

	/// <summary>
	/// Reloads active listings and updates the selection and status.
	/// </summary>
	/// <returns>
	/// A task that completes when the load succeeds or the failure is reported.
	/// </returns>
	[SuppressMessage(
		"Design",
		"CA1031",
		Justification = "The view reports load failures and allows retry.")]
	public async Task LoadJobsAsync()
	{
		if (IsLoading)
		{
			return;
		}

		IsLoading = true;
		StatusMessage = "Loading jobs...";

		try
		{
			List<Job> loadedJobs = await jobQueries.GetJobsAsync()
				.ConfigureAwait(true);
			ApplyLoadedJobs(loadedJobs);
			UpdateLoadedStatus();
		}
		catch (Exception exception)
		{
			StatusMessage = "Unable to load jobs: " + exception.Message;
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>
	/// Opens the selected posting and reports the outcome in the status line.
	/// </summary>
	public void OpenSelectedPosting()
	{
		Uri? address = GetSelectedPostingAddress();
		if (address == null)
		{
			StatusMessage = "Select a job with a valid HTTP or HTTPS posting.";
			return;
		}

		try
		{
			openPosting(address);
			StatusMessage = "Opening selected posting.";
		}
		catch (Exception exception) when (
			exception is Win32Exception or InvalidOperationException)
		{
			StatusMessage = "Unable to open posting: " + exception.Message;
		}
	}

	private void ApplyLoadedJobs(List<Job> loadedJobs)
	{
		int? selectedJobIdentifier = SelectedJob?.Id;
		jobs = loadedJobs;
		OnPropertyChanged(nameof(Jobs));
		SelectedJob = loadedJobs.Find(job => job.Id == selectedJobIdentifier);
		SelectFirstJobIfNeeded(loadedJobs);
	}

	private void SelectFirstJobIfNeeded(List<Job> loadedJobs)
	{
		if (SelectedJob == null && loadedJobs.Count > 0)
		{
			SelectedJob = loadedJobs[0];
		}
	}

	private void UpdateLoadedStatus()
	{
		if (jobs.Count == 0)
		{
			StatusMessage = "No active jobs found.";
		}
		else if (jobs.Count == 1)
		{
			StatusMessage = "Loaded 1 active job.";
		}
		else
		{
			StatusMessage = $"Loaded {jobs.Count} active jobs.";
		}
	}

	private Uri? GetSelectedPostingAddress()
	{
		if (!Uri.TryCreate(
			SelectedJob?.SourceUrl, UriKind.Absolute, out Uri? address))
		{
			return null;
		}

		bool isWebAddress = address.Scheme == Uri.UriSchemeHttps ||
			address.Scheme == Uri.UriSchemeHttp;
		if (!isWebAddress || address.UserInfo.Length > 0)
		{
			return null;
		}

		return address;
	}

	private string GetSelectedDescription()
	{
		if (SelectedJob == null)
		{
			return "Select a job to review.";
		}

		if (string.IsNullOrWhiteSpace(SelectedJob.Description))
		{
			return "No description saved yet. Open the posting for details.";
		}

		return SelectedJob.Description;
	}

	private void OnPropertyChanged(
		[CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(
			this, new PropertyChangedEventArgs(propertyName));
	}
}
