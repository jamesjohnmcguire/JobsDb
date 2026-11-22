/////////////////////////////////////////////////////////////////////////////
// <copyright file="DashboardBindingTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Application.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DigitalZenWorks.JobsDb.Application;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using JobsDbTests;
using NUnit.Framework;

[TestFixture]
[Apartment(ApartmentState.STA)]
internal sealed class DashboardBindingTests
{
	[Test]
	public void Dashboard_ShowsDatabaseJobsAndUpdatesSelection()
	{
		string databasePath = Path.Combine(
			Path.GetTempPath(), "JobsDb-dashboard-" + Guid.NewGuid() + ".db");
		DatabaseTestSchema.Create(databasePath);
		try
		{
			using LibraryManager manager =
				LibraryManager.Initialize(databasePath);
			Job firstJob = new Job
			{
				Title = "Software Developer",
				Company = "Example Company",
				Location = "Tokyo",
				Description = "Build maintainable software and useful tools.",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/1",
				SourceJobId = "1",
				DatePosted = new DateTime(
					2026, 11, 17, 0, 0, 0, DateTimeKind.Utc)
			};
			Job secondJob = new Job
			{
				Title = "Data Engineer",
				Company = "Example Company",
				Description = "Work on data pipelines.",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/2",
				SourceJobId = "2",
				DatePosted = firstJob.DatePosted.AddDays(-1)
			};
			using JobsDbContext preparation = new JobsDbContext(databasePath);
			JobRepository repository = new JobRepository(preparation);
			repository.AddAsync(firstJob).GetAwaiter().GetResult();
			repository.AddAsync(secondJob).GetAwaiter().GetResult();
			List<Uri> opened = new List<Uri>();
			JobsDashboardViewModel viewModel =
				new JobsDashboardViewModel(manager.JobQueries, opened.Add);
			viewModel.LoadJobsAsync().GetAwaiter().GetResult();
			MainWindow window = new MainWindow(viewModel);
			try
			{
				FrameworkElement dashboard = (FrameworkElement)window.Content;
				Size size = new Size(1068, 580);
				dashboard.Measure(size);
				dashboard.Arrange(new Rect(size));
				dashboard.UpdateLayout();
				Dispatcher.CurrentDispatcher.Invoke(
					() => { }, DispatcherPriority.ApplicationIdle);

				DataGrid grid = (DataGrid)window.FindName("JobsGrid");
				TextBox description =
					(TextBox)window.FindName("JobDescription");
				TextBlock status = (TextBlock)window.FindName("StatusText");
				Assert.That(grid.Items.Count, Is.EqualTo(2));
				Assert.That(grid.IsReadOnly, Is.True);
				Assert.That(description.Text, Is.EqualTo(firstJob.Description));
				Assert.That(status.Text, Is.EqualTo("Loaded 2 active jobs."));
				TextBlock title =
					(TextBlock)grid.Columns[0].GetCellContent(grid.Items[0]);
				Assert.That(title.Text, Is.EqualTo(firstJob.Title));

				Button openPosting =
					(Button)window.FindName("OpenPostingButton");
				Assert.That(openPosting.IsEnabled, Is.True);
				openPosting.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
				Assert.That(opened, Has.Count.EqualTo(1));
				Assert.That(
					opened[0].AbsoluteUri, Is.EqualTo(firstJob.SourceUrl));

				grid.SelectedItem = grid.Items[1];
				Dispatcher.CurrentDispatcher.Invoke(
					() => { }, DispatcherPriority.ApplicationIdle);
				Assert.That(
					viewModel.SelectedJob!.Id, Is.EqualTo(secondJob.Id));
				Assert.That(
					description.Text, Is.EqualTo(secondJob.Description));
				viewModel.LoadJobsAsync().GetAwaiter().GetResult();
				Dispatcher.CurrentDispatcher.Invoke(
					() => { }, DispatcherPriority.ApplicationIdle);
				Assert.That(
					viewModel.SelectedJob!.Id, Is.EqualTo(secondJob.Id));

				grid.SelectedItem = grid.Items[0];
				dashboard.UpdateLayout();
				SavePreview(dashboard);
			}
			finally
			{
				window.Close();
			}
		}
		finally
		{
			using JobsDbContext cleanup = new JobsDbContext(databasePath);
			cleanup.Database.EnsureDeleted();
		}
	}

	private static void SavePreview(FrameworkElement dashboard)
	{
		RenderTargetBitmap image = new RenderTargetBitmap(
			(int)dashboard.ActualWidth, (int)dashboard.ActualHeight,
			96, 96, PixelFormats.Pbgra32);
				DrawingVisual drawing = new DrawingVisual();
		using (DrawingContext context = drawing.RenderOpen())
		{
			Rect bounds = new Rect(
				0, 0, dashboard.ActualWidth, dashboard.ActualHeight);
			context.DrawRectangle(new VisualBrush(dashboard), null, bounds);
		}

		image.Render(drawing);
		PngBitmapEncoder encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(image));
		string previewPath = Path.Combine(
			Path.GetTempPath(), "JobsDb-dashboard-preview.png");
		using FileStream output = File.Create(previewPath);
		encoder.Save(output);
		TestContext.AddTestAttachment(
			previewPath, "Database dashboard preview");
	}
}
