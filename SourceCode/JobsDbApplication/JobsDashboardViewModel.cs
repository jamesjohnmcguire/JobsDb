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
using System.Linq;
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
	private readonly IJobPriorityService? jobPriorities;
	private IReadOnlyList<Job> jobs = Array.Empty<Job>();
	private List<Job> visibleJobs = new List<Job>();
	private int selectedReviewTab;
	private Job? selectedJob;
	private string statusMessage = "Ready";
	private bool isLoading;
	private bool isSavingPriority;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobsDashboardViewModel"/>
	/// class.
	/// </summary>
	/// <param name="jobQueries">The service supplying stored listings.</param>
	/// <param name="openPosting">
	/// The browser action, or null to use the default launcher.
	/// </param>
	/// <param name="jobPriorities">The optional priority write service.</param>
	public JobsDashboardViewModel(
		IJobQueryService jobQueries,
		Action<Uri>? openPosting = null,
		IJobPriorityService? jobPriorities = null)
	{
		ArgumentNullException.ThrowIfNull(jobQueries);
		this.jobQueries = jobQueries;
		this.openPosting = openPosting ?? PostingLauncher.Open;
		this.jobPriorities = jobPriorities;
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
	/// Gets the listings in the selected review tab.
	/// </summary>
	public IReadOnlyList<Job> VisibleJobs
	{
		get
		{
			return visibleJobs;
		}
	}

	/// <summary>
	/// Gets or sets the selected tab index. Inbox is first.
	/// </summary>
	public int SelectedReviewTab
	{
		get
		{
			return selectedReviewTab;
		}

		set
		{
			if (selectedReviewTab == value || value is not (0 or 1))
			{
				return;
			}

			selectedReviewTab = value;
			OnPropertyChanged();
			ApplyReviewFilter(null);
			UpdateLoadedStatus();
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
			Uri? address = PostingLauncher.GetWebAddress(SelectedJob?.SourceUrl);
			return address != null;
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
			NotifyOperationAvailability();
		}
	}

	/// <summary>
	/// Gets a value indicating whether the dashboard can reload jobs.
	/// </summary>
	public bool CanLoad
	{
		get
		{
			return !isLoading && !isSavingPriority;
		}
	}

	/// <summary>
	/// Gets a value indicating whether priorities can be edited.
	/// </summary>
	public bool CanEditPriority
	{
		get
		{
			return jobPriorities != null && CanLoad;
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
		if (!CanLoad)
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
		Uri? address = PostingLauncher.GetWebAddress(SelectedJob?.SourceUrl);
		if (address == null)
		{
			StatusMessage = "Select a job with a valid HTTP or HTTPS posting.";
			return;
		}

		OpenPosting(address);
	}

	/// <summary>
	/// Opens a validated web address in the default browser.
	/// </summary>
	/// <param name="address">The selected web link.</param>
	public void OpenPosting(Uri address)
	{
		try
		{
			openPosting(address);
			StatusMessage = "Opening posting link.";
		}
		catch (Exception exception) when (
			exception is Win32Exception or InvalidOperationException)
		{
			StatusMessage = "Unable to open posting: " + exception.Message;
		}
	}

	/// <summary>
	/// Saves a priority for the specified posting and updates the display.
	/// </summary>
	/// <param name="job">The posting whose button was clicked.</param>
	/// <param name="priority">The requested priority.</param>
	/// <param name="nextJobId">The adjacent grid row, if available.</param>
	/// <returns>A task that completes when the update is reported.</returns>
	[SuppressMessage(
		"Design",
		"CA1031",
		Justification = "The view reports write failures and allows retry.")]
	public async Task SetPriorityAsync(
		Job job, JobPriority priority, int? nextJobId)
	{
		if (jobPriorities == null || !CanLoad || !jobs.Contains(job))
		{
			return;
		}

		isSavingPriority = true;
		NotifyOperationAvailability();
		try
		{
			bool saved = await jobPriorities.SetPriorityAsync(job.Id, priority)
				.ConfigureAwait(true);
			if (!saved)
			{
				StatusMessage =
					"This posting is no longer available. Reload jobs.";
				return;
			}

			ApplySavedPriority(job, priority, nextJobId);
		}
		catch (Exception exception)
		{
			StatusMessage = "Unable to save priority: " + exception.Message;
		}
		finally
		{
			isSavingPriority = false;
			NotifyOperationAvailability();
		}
	}

	private void NotifyOperationAvailability()
	{
		OnPropertyChanged(nameof(CanLoad));
		OnPropertyChanged(nameof(CanEditPriority));
	}

	private void ApplyLoadedJobs(List<Job> loadedJobs)
	{
		int? selectedJobIdentifier = SelectedJob?.Id;
		jobs = loadedJobs;
		OnPropertyChanged(nameof(Jobs));
		ApplyReviewFilter(selectedJobIdentifier);
	}

	private void ApplySavedPriority(
		Job job, JobPriority priority, int? nextJobId)
	{
		int rowIndex = visibleJobs.IndexOf(job);
		bool movesToReviewed = rowIndex >= 0 && !IsReviewed(job);
		int? selectedJobIdentifier = SelectedJob?.Id;
		job.Priority = (int)priority;
		if (movesToReviewed)
		{
			ApplyReviewFilter(nextJobId, rowIndex);
			StatusMessage = "Priority saved. Posting moved to Reviewed.";
		}
		else
		{
			ApplyReviewFilter(selectedJobIdentifier);
			StatusMessage = "Priority saved.";
		}
	}

	private static bool IsReviewed(Job job)
	{
		return job.Priority.HasValue ||
			job.Status != ApplicationStatus.NotApplied;
	}

	private void ApplyReviewFilter(int? preferredJobId, int fallbackIndex = 0)
	{
		bool showReviewed = selectedReviewTab == 1;
		visibleJobs = jobs.Where(job => IsReviewed(job) == showReviewed)
			.ToList();
		Job? preferredJob = visibleJobs.Find(job => job.Id == preferredJobId);
		if (preferredJob == null && visibleJobs.Count > 0)
		{
			int index = Math.Min(fallbackIndex, visibleJobs.Count - 1);
			preferredJob = visibleJobs[index];
		}

		OnPropertyChanged(nameof(VisibleJobs));
		SelectedJob = preferredJob;
	}

	private void UpdateLoadedStatus()
	{
		string tab = selectedReviewTab == 1 ? "Reviewed" : "Inbox";
		string label = visibleJobs.Count == 1 ? "posting" : "postings";
		StatusMessage = $"{tab}: {visibleJobs.Count} {label}.";
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
