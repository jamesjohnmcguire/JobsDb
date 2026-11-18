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

	private async void ReloadJobs_Click(object sender, RoutedEventArgs e)
	{
		await viewModel.LoadJobsAsync().ConfigureAwait(true);
	}
}
