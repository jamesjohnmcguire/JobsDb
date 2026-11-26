/////////////////////////////////////////////////////////////////////////////
// <copyright file="MainWindow.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.Win32;

/// <summary>
/// Presents the dashboard and forwards load requests to its view model.
/// </summary>
internal sealed partial class MainWindow : Window
{
	private readonly JobsDashboardViewModel viewModel;
	private readonly string? themePreferencePath;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	/// <param name="viewModel">
	/// The source of the dashboard's data and actions.
	/// </param>
	/// <param name="themePreferencePath">
	/// The preferences file, or null to use the default for this window.
	/// </param>
	internal MainWindow(
		JobsDashboardViewModel viewModel, string? themePreferencePath = null)
	{
		ArgumentNullException.ThrowIfNull(viewModel);
		InitializeComponent();
		this.viewModel = viewModel;
		DataContext = viewModel;
		this.themePreferencePath = themePreferencePath;
		string theme = DashboardThemePreference.Load(themePreferencePath);
		ApplyTheme(theme);
		ThemeSelector.SelectedValue = theme;
		ThemeSelector.SelectionChanged += ThemeSelector_SelectionChanged;
		viewModel.PropertyChanged += ViewModel_PropertyChanged;
		UpdateDescription();
		Loaded += MainWindow_Loaded;
	}

	/// <inheritdoc/>
	protected override void OnClosed(EventArgs e)
	{
		viewModel.PropertyChanged -= ViewModel_PropertyChanged;
		base.OnClosed(e);
	}

	private void ViewModel_PropertyChanged(
		object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(viewModel.SelectedDescription))
		{
			UpdateDescription();
		}
	}

	private void UpdateDescription()
	{
		PostingDescription.Format(
			DescriptionParagraph.Inlines,
			viewModel.SelectedDescription,
			viewModel.OpenPosting);
	}

	// WPF marks runtime Fluent theme selection as experimental.
#pragma warning disable WPF0001
	private void ApplyTheme(string theme)
	{
		ThemeMode mode = theme switch
		{
			"System" => ThemeMode.System,
			"Light" => ThemeMode.Light,
			_ => ThemeMode.Dark
		};

		ThemeMode = mode;
	}

#pragma warning restore WPF0001

	private void ThemeSelector_SelectionChanged(
		object sender, SelectionChangedEventArgs e)
	{
		if (ThemeSelector.SelectedValue is string theme)
		{
			ApplyTheme(theme);
			SaveTheme(theme);
		}
	}

	private void SaveTheme(string theme)
	{
		if (themePreferencePath == null)
		{
			return;
		}

		try
		{
			DashboardThemePreference.Save(themePreferencePath, theme);
		}
		catch (Exception exception) when (
			exception is IOException or UnauthorizedAccessException)
		{
			MessageBox.Show(
				this,
				"The theme changed, but JobsDb could not save the preference.",
				"Theme preference",
				MessageBoxButton.OK,
				MessageBoxImage.Warning);
		}
	}

	private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		Loaded -= MainWindow_Loaded;
		await viewModel.LoadJobsAsync().ConfigureAwait(true);
	}

	private void OpenPosting_Click(object sender, RoutedEventArgs e)
	{
		viewModel.OpenSelectedPosting();
	}

	private async void Priority_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button button &&
			button.DataContext is Job job &&
			button.CommandParameter is JobPriority priority)
		{
			Job? nextJob = FindAdjacentPosting(job);
			await viewModel.SetPriorityAsync(job, priority, nextJob?.Id)
				.ConfigureAwait(true);
		}
	}

	private async void Applied_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button button && button.DataContext is Job job)
		{
			Job? nextJob = FindAdjacentPosting(job);
			await viewModel.ToggleAppliedAsync(job, nextJob?.Id)
				.ConfigureAwait(true);
		}
	}

	private Job? FindAdjacentPosting(Job job)
	{
		int rowIndex = JobsGrid.Items.IndexOf(job);
		if (rowIndex < 0 || JobsGrid.Items.Count < 2)
		{
			return null;
		}

		int nextIndex = rowIndex + 1;
		if (nextIndex == JobsGrid.Items.Count)
		{
			nextIndex = rowIndex - 1;
		}

		return JobsGrid.Items[nextIndex] as Job;
	}

	private async void ImportEmails_Click(object sender, RoutedEventArgs e)
	{
		OpenFolderDialog dialog = new OpenFolderDialog();
		dialog.Title = "Select a folder containing email exports";

		if (dialog.ShowDialog(this) == true)
		{
			await viewModel.ImportEmailsAsync(dialog.FolderName)
				.ConfigureAwait(true);
		}
	}

	private async void ReloadJobs_Click(object sender, RoutedEventArgs e)
	{
		await viewModel.LoadJobsAsync().ConfigureAwait(true);
	}
}
