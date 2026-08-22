/////////////////////////////////////////////////////////////////////////////
// <copyright file="MainWindow.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System.Windows;
using global::JobsDb.Core.Data;
using global::JobsDb.Core.Repositories;
using global::JobsDb.Core.Services;
using JobsDbLibrary.Scrapers;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
internal partial class MainWindow : Window
{
	private readonly IJobRepository jobRepository;
	private readonly ScraperService scraperService;
	private readonly JobsDbContext context;

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
	}
}
