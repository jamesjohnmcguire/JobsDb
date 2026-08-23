/////////////////////////////////////////////////////////////////////////////
// <copyright file="MainWindow.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
internal partial class MainWindow : Window
{
	private readonly JobsDbContext context;
	private readonly IJobRepository jobRepository;
	private readonly ScraperService scraperService;
	private List<Job> allJobs;
	private Job selectedJob;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	/// <param name="jobRepository">Repository for reading and writing job
	/// records.</param>
	/// <param name="scraperService">Service for running the registered
	/// scrapers.</param>
	/// <param name="context">The database context, retained here only for
	/// cases the window needs direct access (e.g. passing to child windows
	/// like credential configuration).</param>
	internal MainWindow(
		IJobRepository jobRepository,
		ScraperService scraperService,
		JobsDbContext context)
	{
		InitializeComponent();

		this.jobRepository = jobRepository;
		this.scraperService = scraperService;
		this.context = context;

		context.Initialize();

		jobRepository = new JobRepository(context);
		CredentialRepository credentialRepository = new(context);

		scraperService = new ScraperService(context);

		// Register scrapers (implement these separately)
		// scraperService.RegisterScraper("LinkedIn", new LinkedInScraper(jobRepository, credentialRepository));
		// scraperService.RegisterScraper("TokyoDev", new TokyoDevScraper(jobRepository, credentialRepository));

		Loaded += MainWindow_Loaded;
	}

	private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		await LoadJobsAsync();
	}

	private async System.Threading.Tasks.Task LoadJobsAsync()
	{
		try
		{
#if FUTURE_UI
			StatusBarText.Text = "Loading jobs...";
#endif
			allJobs = await jobRepository.GetActiveJobsAsync();
			UpdateJobsList(allJobs);
#if FUTURE_UI
			JobCountText.Text = $"{allJobs.Count} jobs";
			StatusBarText.Text = "Ready";
#endif
		}
		catch (Exception ex)
		{
#if FUTURE_UI
			MessageBox.Show($"Error loading jobs: {ex.Message}", "Error",
				MessageBoxButton.OK, MessageBoxImage.Error);
			StatusBarText.Text = "Error loading jobs";
#endif
		}
	}

	private void UpdateJobsList(List<Job> jobs)
	{
#if FUTURE_UI
		JobsListBox.ItemsSource = jobs;
		JobCountText.Text = $"{jobs.Count} jobs";
#endif
	}

	private void JobsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
#if FUTURE_UI
		if (JobsListBox.SelectedItem is Job job)
		{
			selectedJob = job;
			DisplayJobDetails(job);
		}
#endif
	}

	private void DisplayJobDetails(Job job)
	{
#if FUTURE_UI
		JobTitle.Text = job.Title;
		JobCompany.Text = job.Company;
		JobLocation.Text = job.Location ?? "Location not specified";
		JobDescription.Text = job.Description ?? "No description available";
		JobRequirements.Text = job.Requirements ?? "No requirements listed";
		NotesTextBox.Text = job.Notes ?? string.Empty;
#endif

		// Set salary info
		if (job.SalaryMin.HasValue || job.SalaryMax.HasValue)
		{
			var currency = job.SalaryCurrency ?? "JPY";
			var min = job.SalaryMin?.ToString("N0") ?? "N/A";
			var max = job.SalaryMax?.ToString("N0") ?? "N/A";
#if FUTURE_UI
			JobSalary.Text = $"{min} - {max} {currency}";
#endif
		}
		else
		{
#if FUTURE_UI
			JobSalary.Text = "Not specified";
#endif
		}

#if FUTURE_UI
		// Set status
		StatusComboBox.SelectedIndex = (int)job.Status;

		// Set priority
		PriorityComboBox.SelectedIndex = job.Priority ?? 0;
#endif
	}

	private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
#if FUTURE_UI
		var searchTerm = SearchBox.Text;

		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			UpdateJobsList(allJobs);
		}
		else
		{
			var filtered = await jobRepository.SearchAsync(searchTerm);
			UpdateJobsList(filtered);
		}
#endif
	}

	private void ClearSearch_Click(object sender, RoutedEventArgs e)
	{
#if FUTURE_UI
		SearchBox.Clear();
#endif
		UpdateJobsList(allJobs);
	}

	private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
	{
#if FUTURE_UI
		if (StatusFilter.SelectedItem is ComboBoxItem item)
		{
			var status = item.Content.ToString();

			if (status == "All")
			{
				UpdateJobsList(allJobs);
			}
			else
			{
				var statusEnum = Enum.Parse<ApplicationStatus>(status.Replace(" ", ""));
				var filtered = await jobRepository.GetByStatusAsync(statusEnum);
				UpdateJobsList(filtered);
			}
		}
#endif
	}

	private async void StatusComboBox_Changed(object sender, SelectionChangedEventArgs e)
	{
#if FUTURE_UI
		if (selectedJob != null && StatusComboBox.SelectedIndex >= 0)
		{
			selectedJob.Status = (ApplicationStatus)StatusComboBox.SelectedIndex;

			if (selectedJob.Status == ApplicationStatus.Applied && !selectedJob.DateApplied.HasValue)
			{
				selectedJob.DateApplied = DateTime.UtcNow;
			}

			await jobRepository.UpdateAsync(selectedJob);
			await LoadJobsAsync();
		}
#endif
	}

	private async void PriorityComboBox_Changed(object sender, SelectionChangedEventArgs e)
	{
#if FUTURE_UI
		if (selectedJob != null && PriorityComboBox.SelectedIndex >= 0)
		{
			selectedJob.Priority = PriorityComboBox.SelectedIndex == 0 ? null : PriorityComboBox.SelectedIndex;
			await jobRepository.UpdateAsync(selectedJob);
		}
#endif
	}

	private async void NotesTextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		if (selectedJob != null)
		{
#if FUTURE_UI
			selectedJob.Notes = NotesTextBox.Text;
#endif
			await jobRepository.UpdateAsync(selectedJob);
		}
	}

	private void OpenUrl_Click(object sender, RoutedEventArgs e)
	{
		if (selectedJob != null && !string.IsNullOrEmpty(selectedJob.SourceUrl))
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = selectedJob.SourceUrl,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
#if FUTURE_UI
				MessageBox.Show($"Error opening URL: {ex.Message}", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
#endif
			}
		}
	}

	private async void MarkApplied_Click(object sender, RoutedEventArgs e)
	{
		if (selectedJob != null)
		{
			selectedJob.Status = ApplicationStatus.Applied;
			selectedJob.DateApplied = DateTime.UtcNow;
			await jobRepository.UpdateAsync(selectedJob);
			DisplayJobDetails(selectedJob);
			await LoadJobsAsync();
			MessageBox.Show("Job marked as applied!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}

	private async void Archive_Click(object sender, RoutedEventArgs e)
	{
		if (selectedJob != null)
		{
#if FUTURE_UI
			var result = MessageBox.Show("Are you sure you want to archive this job?",
				"Confirm Archive", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				selectedJob.IsArchived = true;
				await jobRepository.UpdateAsync(selectedJob);
				await LoadJobsAsync();
			}
#endif
		}
	}

	private async void RunAllScrapers_Click(object sender, RoutedEventArgs e)
	{
#if FUTURE_UI
		StatusBarText.Text = "Running all scrapers...";
#endif

		try
		{
			var results = await scraperService.RunAllScrapersAsync();
			var totalAdded = results.Sum(r => r.JobsAdded);
			var totalUpdated = results.Sum(r => r.JobsUpdated);

			await LoadJobsAsync();

			MessageBox.Show($"Scraping complete!\nNew jobs: {totalAdded}\nUpdated jobs: {totalUpdated}",
				"Scraper Results", MessageBoxButton.OK, MessageBoxImage.Information);

#if FUTURE_UI
			StatusBarText.Text = "Scraping complete";
#endif
		}
		catch (Exception ex)
		{
#if FUTURE_UI
			MessageBox.Show($"Error running scrapers: {ex.Message}", "Error",
				MessageBoxButton.OK, MessageBoxImage.Error);
			StatusBarText.Text = "Scraping failed";
#endif
		}
	}

	private async void RunLinkedIn_Click(object sender, RoutedEventArgs e)
	{
		await RunSingleScraperAsync("LinkedIn");
	}

	private async void RunTokyoDev_Click(object sender, RoutedEventArgs e)
	{
		await RunSingleScraperAsync("TokyoDev");
	}

	private async System.Threading.Tasks.Task RunSingleScraperAsync(string source)
	{
#if FUTURE_UI
		StatusBarText.Text = $"Running {source} scraper...";
#endif

		try
		{
			var result = await scraperService.RunScraperAsync(source);

			await LoadJobsAsync();

#if FUTURE_UI
			MessageBox.Show($"{source} scraping complete!\nNew jobs: {result.JobsAdded}\nUpdated jobs: {result.JobsUpdated}",
				"Scraper Results", MessageBoxButton.OK, MessageBoxImage.Information);

			StatusBarText.Text = $"{source} scraping complete";
#endif
		}
		catch (Exception ex)
		{
#if FUTURE_UI
			MessageBox.Show($"Error running {source} scraper: {ex.Message}", "Error",
				MessageBoxButton.OK, MessageBoxImage.Error);
			StatusBarText.Text = $"{source} scraping failed";
#endif
		}
	}

	private void ConfigureCredentials_Click(object sender, RoutedEventArgs e)
	{
#if FUTURE_UI
		var credentialsWindow = new CredentialsWindow(context);
		credentialsWindow.Owner = this;
		credentialsWindow.ShowDialog();
#endif
	}

	private void Settings_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Settings window coming soon!", "Settings",
			MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private async void ShowAllJobs_Click(object sender, RoutedEventArgs e)
	{
		await LoadJobsAsync();
	}

	private async void ShowActiveOnly_Click(object sender, RoutedEventArgs e)
	{
		var active = allJobs.Where(j => !j.IsArchived).ToList();
		UpdateJobsList(active);
	}

	private async void Refresh_Click(object sender, RoutedEventArgs e)
	{
		await LoadJobsAsync();
	}

	private void Exit_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
