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
	private IReadOnlyList<Job> jobs = Array.Empty<Job>();
	private Job? selectedJob;
	private string statusMessage = "Ready";
	private bool isLoading;

	public JobsDashboardViewModel(IJobQueryService jobQueries)
	{
		ArgumentNullException.ThrowIfNull(jobQueries);
		this.jobQueries = jobQueries;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public IReadOnlyList<Job> Jobs
	{
		get
		{
			return jobs;
		}
	}

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
		}
	}

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

	public bool CanLoad
	{
		get
		{
			return !isLoading;
		}
	}

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
			int? selectedJobIdentifier = SelectedJob?.Id;
			jobs = loadedJobs;
			OnPropertyChanged(nameof(Jobs));
			SelectedJob = loadedJobs.Find(
				job => job.Id == selectedJobIdentifier);
			if (SelectedJob == null && loadedJobs.Count > 0)
			{
				SelectedJob = loadedJobs[0];
			}

			if (loadedJobs.Count == 0)
			{
				StatusMessage = "No active jobs found.";
			}
			else if (loadedJobs.Count == 1)
			{
				StatusMessage = "Loaded 1 active job.";
			}
			else
			{
				StatusMessage = $"Loaded {loadedJobs.Count} active jobs.";
			}
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

	private void OnPropertyChanged(
		[CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(
			this, new PropertyChangedEventArgs(propertyName));
	}
}
