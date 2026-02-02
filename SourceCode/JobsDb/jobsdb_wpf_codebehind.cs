using JobsDb.Core.Data;
}
namespace JobsDb
{
	using JobsDb.Core.Data;
	using JobsDb.Core.Models;
	using JobsDb.Core.Repositories;
	using JobsDb.Core.Services;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;

    public partial class MainWindow : Window
    {
        private readonly JobsDbContext _context;
        private readonly IJobRepository _jobRepository;
        private readonly ScraperService _scraperService;
        private List<Job> _allJobs;
        private Job _selectedJob;

        public MainWindow()
        {
            InitializeComponent();
            
            _context = new JobsDbContext();
            _context.Initialize();
            
            _jobRepository = new JobRepository(_context);
            var credentialRepository = new CredentialRepository(_context);
            
            _scraperService = new ScraperService(_context);
            
            // Register scrapers (implement these separately)
            // _scraperService.RegisterScraper("LinkedIn", new LinkedInScraper(_jobRepository, credentialRepository));
            // _scraperService.RegisterScraper("TokyoDev", new TokyoDevScraper(_jobRepository, credentialRepository));
            
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
                StatusBarText.Text = "Loading jobs...";
                _allJobs = await _jobRepository.GetActiveJobsAsync();
                UpdateJobsList(_allJobs);
                JobCountText.Text = $"{_allJobs.Count} jobs";
                StatusBarText.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading jobs: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Text = "Error loading jobs";
            }
        }

        private void UpdateJobsList(List<Job> jobs)
        {
            JobsListBox.ItemsSource = jobs;
            JobCountText.Text = $"{jobs.Count} jobs";
        }

        private void JobsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JobsListBox.SelectedItem is Job job)
            {
                _selectedJob = job;
                DisplayJobDetails(job);
            }
        }

        private void DisplayJobDetails(Job job)
        {
            JobTitle.Text = job.Title;
            JobCompany.Text = job.Company;
            JobLocation.Text = job.Location ?? "Location not specified";
            JobDescription.Text = job.Description ?? "No description available";
            JobRequirements.Text = job.Requirements ?? "No requirements listed";
            NotesTextBox.Text = job.Notes ?? string.Empty;
            
            // Set salary info
            if (job.SalaryMin.HasValue || job.SalaryMax.HasValue)
            {
                var currency = job.SalaryCurrency ?? "JPY";
                var min = job.SalaryMin?.ToString("N0") ?? "N/A";
                var max = job.SalaryMax?.ToString("N0") ?? "N/A";
                JobSalary.Text = $"{min} - {max} {currency}";
            }
            else
            {
                JobSalary.Text = "Not specified";
            }
            
            // Set status
            StatusComboBox.SelectedIndex = (int)job.Status;
            
            // Set priority
            PriorityComboBox.SelectedIndex = job.Priority ?? 0;
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchBox.Text;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                UpdateJobsList(_allJobs);
            }
            else
            {
                var filtered = await _jobRepository.SearchAsync(searchTerm);
                UpdateJobsList(filtered);
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            UpdateJobsList(_allJobs);
        }

        private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilter.SelectedItem is ComboBoxItem item)
            {
                var status = item.Content.ToString();
                
                if (status == "All")
                {
                    UpdateJobsList(_allJobs);
                }
                else
                {
                    var statusEnum = Enum.Parse<ApplicationStatus>(status.Replace(" ", ""));
                    var filtered = await _jobRepository.GetByStatusAsync(statusEnum);
                    UpdateJobsList(filtered);
                }
            }
        }

        private async void StatusComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedJob != null && StatusComboBox.SelectedIndex >= 0)
            {
                _selectedJob.Status = (ApplicationStatus)StatusComboBox.SelectedIndex;
                
                if (_selectedJob.Status == ApplicationStatus.Applied && !_selectedJob.DateApplied.HasValue)
                {
                    _selectedJob.DateApplied = DateTime.UtcNow;
                }
                
                await _jobRepository.UpdateAsync(_selectedJob);
                await LoadJobsAsync();
            }
        }

        private async void PriorityComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedJob != null && PriorityComboBox.SelectedIndex >= 0)
            {
                _selectedJob.Priority = PriorityComboBox.SelectedIndex == 0 ? null : PriorityComboBox.SelectedIndex;
                await _jobRepository.UpdateAsync(_selectedJob);
            }
        }

        private async void NotesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_selectedJob != null)
            {
                _selectedJob.Notes = NotesTextBox.Text;
                await _jobRepository.UpdateAsync(_selectedJob);
            }
        }

        private void OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedJob != null && !string.IsNullOrEmpty(_selectedJob.SourceUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _selectedJob.SourceUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening URL: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void MarkApplied_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedJob != null)
            {
                _selectedJob.Status = ApplicationStatus.Applied;
                _selectedJob.DateApplied = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(_selectedJob);
                DisplayJobDetails(_selectedJob);
                await LoadJobsAsync();
                MessageBox.Show("Job marked as applied!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void Archive_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedJob != null)
            {
                var result = MessageBox.Show("Are you sure you want to archive this job?", 
                    "Confirm Archive", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _selectedJob.IsArchived = true;
                    await _jobRepository.UpdateAsync(_selectedJob);
                    await LoadJobsAsync();
                }
            }
        }

        private async void RunAllScrapers_Click(object sender, RoutedEventArgs e)
        {
            StatusBarText.Text = "Running all scrapers...";
            
            try
            {
                var results = await _scraperService.RunAllScrapersAsync();
                var totalAdded = results.Sum(r => r.JobsAdded);
                var totalUpdated = results.Sum(r => r.JobsUpdated);
                
                await LoadJobsAsync();
                
                MessageBox.Show($"Scraping complete!\nNew jobs: {totalAdded}\nUpdated jobs: {totalUpdated}", 
                    "Scraper Results", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusBarText.Text = "Scraping complete";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running scrapers: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Text = "Scraping failed";
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
            StatusBarText.Text = $"Running {source} scraper...";
            
            try
            {
                var result = await _scraperService.RunScraperAsync(source);
                
                await LoadJobsAsync();
                
                MessageBox.Show($"{source} scraping complete!\nNew jobs: {result.JobsAdded}\nUpdated jobs: {result.JobsUpdated}", 
                    "Scraper Results", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusBarText.Text = $"{source} scraping complete";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running {source} scraper: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Text = $"{source} scraping failed";
            }
        }

        private void ConfigureCredentials_Click(object sender, RoutedEventArgs e)
        {
            var credentialsWindow = new CredentialsWindow(_context);
            credentialsWindow.Owner = this;
            credentialsWindow.ShowDialog();
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
            var active = _allJobs.Where(j => !j.IsArchived).ToList();
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
}
