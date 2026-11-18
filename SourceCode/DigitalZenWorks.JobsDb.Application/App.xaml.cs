/////////////////////////////////////////////////////////////////////////////
// <copyright file="App.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.Windows;
using DigitalZenWorks.JobsDb.Library;
using Application = System.Windows.Application;

/// <summary>
/// Initializes the library and presents the application window.
/// </summary>
internal partial class App : Application
{
	private LibraryManager? libraryManager;

	/// <inheritdoc/>
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		try
		{
			libraryManager = LibraryManager.Initialize();
		}
		catch (InvalidOperationException exception)
		{
			MessageBox.Show(
				exception.Message,
				"JobsDb startup failed",
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			Shutdown(1);
			return;
		}

		JobsDashboardViewModel viewModel =
			new JobsDashboardViewModel(libraryManager.JobQueries);
		MainWindow mainWindow = new MainWindow(viewModel);
		mainWindow.Show();
	}

	/// <inheritdoc/>
	protected override void OnExit(ExitEventArgs e)
	{
		libraryManager?.Dispose();
		base.OnExit(e);
	}
}
