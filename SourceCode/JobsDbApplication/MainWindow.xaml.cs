/////////////////////////////////////////////////////////////////////////////
// <copyright file="MainWindow.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.Windows;

/// <summary>
/// Presents the dashboard and forwards load requests to its view model.
/// </summary>
internal sealed partial class MainWindow : Window
{
	private readonly JobsDashboardViewModel viewModel;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	/// <param name="viewModel">
	/// The source of the dashboard's data and actions.
	/// </param>
	internal MainWindow(JobsDashboardViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(viewModel);
		InitializeComponent();
		this.viewModel = viewModel;
		DataContext = viewModel;
		Loaded += MainWindow_Loaded;
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

	private async void ReloadJobs_Click(object sender, RoutedEventArgs e)
	{
		await viewModel.LoadJobsAsync().ConfigureAwait(true);
	}
}
