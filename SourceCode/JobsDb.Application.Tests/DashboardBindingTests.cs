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
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using DigitalZenWorks.JobsDb.Application;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Services;
using JobsDbTests;
using Moq;
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
				Description = "See (https://example.com/team) for details. " +
					"More at https://example.com/about.",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/1",
				SourceJobId = "1",
				DatePosted = new DateTime(
					2026, 11, 17, 0, 0, 0, DateTimeKind.Utc)
			};
			Job secondJob = new Job
			{
				Title = "Senior Solutions Architect: Enterprise Database " +
					"Modernization and Distributed Systems Architecture " +
					"for Customers Across Japan",
				Company = "Example Company",
				Description = "Work on data pipelines.",
				Source = "TestSource",
				SourceUrl = "https://example.com/jobs/2",
				SourceJobId = "2",
				DatePosted = firstJob.DatePosted.AddDays(1)
			};
			Job appliedJob = new Job
			{
				Title = "Applied Posting",
				Company = "Example Company",
				Source = "TestSource",
				SourceJobId = "3",
				SourceUrl = "https://example.com/jobs/3",
				DatePosted = secondJob.DatePosted.AddDays(-1),
				Status = ApplicationStatus.Interviewing
			};
			using JobsDbContext preparation = new JobsDbContext(databasePath);
			JobRepository repository = new JobRepository(preparation);
			repository.AddAsync(firstJob).GetAwaiter().GetResult();
			repository.AddAsync(secondJob).GetAwaiter().GetResult();
			repository.AddAsync(appliedJob).GetAwaiter().GetResult();
			List<Uri> opened = new List<Uri>();
			JobsDashboardViewModel viewModel =
				new JobsDashboardViewModel(
				manager.JobQueries,
					opened.Add,
					manager.JobPriorities,
					manager.JobApplications);
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

				Button import = (Button)window.FindName("ImportEmailsButton");
				Assert.That(import.Content, Is.EqualTo("Import Emails"));
				Assert.That(import.IsEnabled, Is.True);
				DataGrid grid = (DataGrid)window.FindName("JobsGrid");
				RichTextBox description =
					(RichTextBox)window.FindName("JobDescription");
				TextBlock status = (TextBlock)window.FindName("StatusText");
				Assert.That(grid.Items.Count, Is.EqualTo(2));
				Assert.That(grid.IsReadOnly, Is.True);
				Assert.That(
					ReadDescription(description),
					Is.EqualTo(firstJob.Description));
				Assert.That(status.Text, Is.EqualTo("Inbox: 2 postings."));
				TextBlock title =
					(TextBlock)grid.Columns[0].GetCellContent(grid.Items[0]);
				Assert.That(title.Text, Is.EqualTo(firstJob.Title));

				CheckLongTitleCell(grid, (Job)grid.Items[1]);
				CheckPostingLinks(window, description, opened, firstJob);

				grid.SelectedItem = grid.Items[1];
				Dispatcher.CurrentDispatcher.Invoke(
					() => { }, DispatcherPriority.ApplicationIdle);
				Assert.That(
					viewModel.SelectedJob!.Id, Is.EqualTo(secondJob.Id));
				Assert.That(
					ReadDescription(description),
					Is.EqualTo(secondJob.Description));
				viewModel.LoadJobsAsync().GetAwaiter().GetResult();
				Dispatcher.CurrentDispatcher.Invoke(
					() => { }, DispatcherPriority.ApplicationIdle);
				Assert.That(
					viewModel.SelectedJob!.Id, Is.EqualTo(secondJob.Id));

				TabControl tabs = (TabControl)window.FindName("ReviewTabs");
				CheckPriorityButton(
					grid, manager, secondJob.Id, firstJob.Id);
				Assert.That(tabs.SelectedIndex, Is.Zero);
				Assert.That(grid.Items.Count, Is.EqualTo(1));
				Assert.That(viewModel.SelectedJob!.Id, Is.EqualTo(firstJob.Id));

				tabs.SelectedIndex = 1;
				dashboard.UpdateLayout();
				Assert.That(grid.Items.Count, Is.EqualTo(2));
				Assert.That(((Job)grid.Items[0]).Id, Is.EqualTo(appliedJob.Id));
				Assert.That(((Job)grid.Items[1]).Id, Is.EqualTo(secondJob.Id));
				Assert.That(
					viewModel.SelectedJob!.Id, Is.EqualTo(appliedJob.Id));
				grid.SelectedItem = grid.Items[1];
				dashboard.UpdateLayout();
				Button reviewedPriority = FindPriorityButton(grid, 1);
				Assert.That(reviewedPriority.Tag, Is.EqualTo("Selected"));
				CheckLongTitleCell(grid, (Job)grid.Items[1]);
				CheckColumnAlignment(grid);
				SavePreview(dashboard);
				CheckAppliedToggle(grid, tabs, manager, firstJob.Id);
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

	/// <summary>
	/// Checks the default theme, live switching, and saved selection.
	/// </summary>
	[Test]
	public void Dashboard_ChangesThemeAndRestoresSavedChoice()
	{
		string preferencePath = Path.Combine(
			Path.GetTempPath(), "JobsDb-theme-" + Guid.NewGuid() + ".txt");
		IJobQueryService jobQueries = Mock.Of<IJobQueryService>();
		JobsDashboardViewModel viewModel = new JobsDashboardViewModel(
			jobQueries);

		try
		{
			CheckThemeSelection(viewModel, preferencePath);
			CheckSavedTheme(viewModel, preferencePath);
		}
		finally
		{
			File.Delete(preferencePath);
		}
	}

	private static void CheckThemeSelection(
		JobsDashboardViewModel viewModel, string preferencePath)
	{
		MainWindow window = new MainWindow(viewModel, preferencePath);
		try
		{
			ComboBox selector = (ComboBox)window.FindName("ThemeSelector");
			Assert.That(selector.SelectedValue, Is.EqualTo("Dark"));
			Assert.That(File.Exists(preferencePath), Is.False);
			Color dark = ReadDashboardBackground(window);

			selector.SelectedValue = "System";
			Assert.That(File.ReadAllText(preferencePath), Is.EqualTo("System"));

			selector.SelectedValue = "Light";
			Assert.That(File.ReadAllText(preferencePath), Is.EqualTo("Light"));
			Color light = ReadDashboardBackground(window);

			Assert.That(light.R, Is.GreaterThan(dark.R));
		}
		finally
		{
			window.Close();
		}
	}

	private static void CheckSavedTheme(
		JobsDashboardViewModel viewModel, string preferencePath)
	{
		MainWindow window = new MainWindow(viewModel, preferencePath);
		try
		{
			ComboBox selector = (ComboBox)window.FindName("ThemeSelector");
			Assert.That(selector.SelectedValue, Is.EqualTo("Light"));
		}
		finally
		{
			window.Close();
		}
	}

	private static Color ReadDashboardBackground(MainWindow window)
	{
		DockPanel dashboard = (DockPanel)window.Content;
		window.ApplyTemplate();
		dashboard.UpdateLayout();
		SolidColorBrush background = (SolidColorBrush)dashboard.Background;
		return background.Color;
	}

	private static string ReadDescription(RichTextBox description)
	{
		TextRange text = new TextRange(
			description.Document.ContentStart, description.Document.ContentEnd);
		return text.Text.TrimEnd('\r', '\n');
	}

	private static void CheckPostingLinks(
		MainWindow window, RichTextBox description, List<Uri> opened, Job job)
	{
		Hyperlink source = (Hyperlink)window.FindName("PostingAddressLink");
		Assert.That(source.IsEnabled, Is.True);
		source.RaiseEvent(new RoutedEventArgs(Hyperlink.ClickEvent));
		Assert.That(opened, Has.Count.EqualTo(1));
		Assert.That(opened[0].AbsoluteUri, Is.EqualTo(job.SourceUrl));

		Assert.That(description.IsDocumentEnabled, Is.True);
		Paragraph paragraph =
			(Paragraph)description.Document.Blocks.FirstBlock;
		Inline? candidate = paragraph.Inlines.FirstInline?.NextInline;
		Assert.That(candidate, Is.TypeOf<Hyperlink>());
		Hyperlink link = (Hyperlink)candidate!;
		Assert.That(
			link.NavigateUri.AbsoluteUri,
			Is.EqualTo("https://example.com/team"));
		Inline? finalContent = paragraph.Inlines.LastInline?.PreviousInline;
		Assert.That(finalContent, Is.TypeOf<Hyperlink>());
		Hyperlink lastLink = (Hyperlink)finalContent!;
		Assert.That(
			lastLink.NavigateUri.AbsoluteUri,
			Is.EqualTo("https://example.com/about"));
		RequestNavigateEventArgs request = new RequestNavigateEventArgs(
			link.NavigateUri, string.Empty);
		request.RoutedEvent = Hyperlink.RequestNavigateEvent;
		link.RaiseEvent(request);
		Assert.That(request.Handled, Is.True);
		Assert.That(opened, Has.Count.EqualTo(2));
		Assert.That(opened[1], Is.EqualTo(link.NavigateUri));
	}

	private static void CheckLongTitleCell(DataGrid grid, Job job)
	{
		TextBlock title = (TextBlock)grid.Columns[0].GetCellContent(job);
		FrameworkElement company = grid.Columns[1].GetCellContent(job);
		double companyStart = company.TranslatePoint(default, grid).X;
		Point titleRight = new Point(title.ActualWidth, 0);
		double titleEnd = title.TranslatePoint(titleRight, grid).X;

		Assert.That(title.Text, Is.EqualTo(job.Title));
		Assert.That(titleEnd, Is.LessThanOrEqualTo(companyStart));
		Assert.That(title.ToolTip, Is.EqualTo(job.Title));
		Assert.That(
			title.TextTrimming,
			Is.EqualTo(TextTrimming.CharacterEllipsis));
	}

	private static void CheckColumnAlignment(DataGrid grid)
	{
		foreach (DataGridColumn column in grid.Columns)
		{
			FrameworkElement firstCell = column.GetCellContent(grid.Items[0]);
			FrameworkElement secondCell = column.GetCellContent(grid.Items[1]);
			double firstOffset = firstCell.TranslatePoint(default, grid).X;
			double secondOffset = secondCell.TranslatePoint(default, grid).X;

			Assert.That(secondOffset, Is.EqualTo(firstOffset).Within(0.5));
		}
	}

	private static void CheckAppliedToggle(
		DataGrid grid, TabControl tabs, LibraryManager manager, int jobId)
	{
		tabs.SelectedIndex = 0;
		grid.UpdateLayout();
		Button applied = FindAppliedButton(grid, 0);
		Assert.That(applied.Content, Is.EqualTo("\u2717"));
		Assert.That(applied.IsEnabled, Is.True);
		ClickAppliedButton(applied);
		Assert.That(grid.Items.Count, Is.Zero);
		CheckStoredStatus(manager, jobId, ApplicationStatus.Applied);

		tabs.SelectedIndex = 1;
		grid.UpdateLayout();
		Button interviewing = FindAppliedButton(grid, 0);
		Assert.That(interviewing.IsEnabled, Is.False);
		applied = FindAppliedButton(grid, 1);
		Assert.That(applied.Content, Is.EqualTo("\u2713"));
		Assert.That(applied.IsEnabled, Is.True);
		ClickAppliedButton(applied);
		Assert.That(grid.Items.Count, Is.EqualTo(2));
		CheckStoredStatus(manager, jobId, ApplicationStatus.NotApplied);

		tabs.SelectedIndex = 0;
		Assert.That(((Job)grid.Items[0]).Id, Is.EqualTo(jobId));
	}

	private static void ClickAppliedButton(Button button)
	{
		button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		Dispatcher.CurrentDispatcher.Invoke(
			() => { }, DispatcherPriority.ApplicationIdle);
	}

	private static void CheckStoredStatus(
		LibraryManager manager, int jobId, ApplicationStatus expected)
	{
		List<Job> jobs = manager.JobQueries.GetJobsAsync()
			.GetAwaiter().GetResult();
		Job job = jobs.Find(candidate => candidate.Id == jobId)!;
		Assert.That(job.Status, Is.EqualTo(expected));
	}

	private static Button FindAppliedButton(DataGrid grid, int row)
	{
		DataGridTemplateColumn column =
			(DataGridTemplateColumn)grid.Columns[^2];
		ContentPresenter cell =
			(ContentPresenter)column.GetCellContent(grid.Items[row]);
		cell.ApplyTemplate();
		return (Button)column.CellTemplate.FindName("AppliedToggle", cell);
	}

	private static void CheckPriorityButton(
		DataGrid grid,
		LibraryManager manager,
		int jobId,
		int otherJobId)
	{
		Button priority = FindPriorityButton(grid, 1);
		Assert.That(priority.IsEnabled, Is.True);
		Assert.That(priority.Tag, Is.Null);
		priority.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		Dispatcher.CurrentDispatcher.Invoke(
			() => { }, DispatcherPriority.ApplicationIdle);

		List<Job> savedJobs = manager.JobQueries.GetJobsAsync()
			.GetAwaiter().GetResult();
		Job savedJob = savedJobs.Find(job => job.Id == jobId)!;
		Assert.That(savedJob.Priority, Is.EqualTo((int)JobPriority.Medium));
		Job otherJob = savedJobs.Find(job => job.Id == otherJobId)!;
		Assert.That(otherJob.Priority, Is.Null);
	}

	private static Button FindPriorityButton(DataGrid grid, int row)
	{
		DataGridTemplateColumn column =
			(DataGridTemplateColumn)grid.Columns[^1];
		ContentPresenter cell = (ContentPresenter)column.GetCellContent(
			grid.Items[row]);
		cell.ApplyTemplate();
		return (Button)column.CellTemplate.FindName("MediumPriority", cell);
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
