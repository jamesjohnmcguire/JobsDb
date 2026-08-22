/////////////////////////////////////////////////////////////////////////////
// <copyright file="App.xaml.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System.Windows;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using DigitalZenWorks.JobsDb.Library.Services;
using Application = System.Windows.Application;

/// <summary>
/// Interaction logic for App.xaml. Acts as the composition root: builds the
/// shared services once at startup and hands them to MainWindow, rather than
/// each window constructing its own dependencies.
/// </summary>
internal partial class App : Application
{
	/// <inheritdoc/>
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		JobsDbContext context = new();
		context.Initialize(); // Applies pending migrations.

		IJobRepository jobRepository = new JobRepository(context);
		ICredentialRepository credentialRepository =
			new CredentialRepository(context);

		ConfigurationManager configManager = new();
		CookieManager cookieManager = new();

		ScraperService scraperService = new(context);
		scraperService.RegisterScraper(
			"LinkedIn",
			new LinkedInScraper(
				jobRepository, credentialRepository, cookieManager, configManager));
		scraperService.RegisterScraper(
			"TokyoDev",
			new TokyoDevScraperSelenium(
				jobRepository, credentialRepository, cookieManager, configManager));

		MainWindow mainWindow =
			new(jobRepository, scraperService, context);
		mainWindow.Show();
	}
}
