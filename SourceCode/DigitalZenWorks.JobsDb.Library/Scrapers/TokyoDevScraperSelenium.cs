/////////////////////////////////////////////////////////////////////////////
// <copyright file="TokyoDevScraperSelenium.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Services;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

/// <summary>
/// TokyoDev scraper using Selenium WebDriver to bypass bot detection
/// This behaves like a real browser and should work even with Cloudflare/bot protection.
/// </summary>
public class TokyoDevScraperSelenium : JobScraperBase
{
	private IWebDriver driver = null!;
	private readonly CookieManager cookieManager;
	private readonly ConfigurationManager configManager;
	private const string BaseUrl = "https://www.tokyodev.com";
	private const string JobsUrl = "https://www.tokyodev.com/jobs";

	/// <summary>
	/// Initializes a new instance of the <see cref="TokyoDevScraperSelenium"/>
	/// class.
	/// </summary>
	/// <param name="jobRepository">The repository for stored listings.</param>
	/// <param name="credentialRepository">
	/// The repository for stored source credentials.
	/// </param>
	/// <param name="cookieManager">
	/// The cookie manager, or null to use the default.
	/// </param>
	/// <param name="configManager">
	/// The configuration manager, or null to use the default.
	/// </param>
	public TokyoDevScraperSelenium(
		IJobRepository jobRepository,
		ICredentialRepository credentialRepository,
		CookieManager cookieManager = null,
		ConfigurationManager configManager = null)
		: base(jobRepository, credentialRepository, "TokyoDev")
	{
		this.cookieManager = cookieManager ?? new CookieManager();
		this.configManager = configManager ?? new ConfigurationManager();
	}

	/// <inheritdoc/>
	public override async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ScraperResult result = new ScraperResult();

		try
		{
			using IWebDriver sessionDriver = CreateDriver();
			driver = sessionDriver;
			ConfigureDriver(sessionDriver);

			// Navigate to jobs page
			Console.WriteLine($"Navigating to {JobsUrl}...");
			driver.Navigate().GoToUrl(JobsUrl);

			// Wait for page to load
			Thread.Sleep(3000);

			// Scroll down to trigger any lazy loading
			ScrollPage();

			// Get the rendered HTML
			string pageSource = driver.PageSource;

			// Save for debugging
			System.IO.File.WriteAllText("tokyodev_scraped.html", pageSource);
			Console.WriteLine("✓ Page source saved to tokyodev_scraped.html");

			// Parse with HtmlAgilityPack
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(pageSource);

			// Try multiple selector strategies
			List<HtmlNode> jobNodes = FindJobNodes(doc);

			if (jobNodes == null || !jobNodes.Any())
			{
				Console.WriteLine("❌ No job nodes found. Check tokyodev_scraped.html");
				Console.WriteLine("Page title: " + driver.Title);
				Console.WriteLine("Current URL: " + driver.Url);

				result.Success = false;
				result.ErrorMessage = "No job listings found on page";
				return result;
			}

			Console.WriteLine($"✓ Found {jobNodes.Count} potential job listings");

			foreach (HtmlNode jobNode in jobNodes)
			{
				try
				{
					Job job = ParseJobNode(jobNode);

					if (job != null && !string.IsNullOrEmpty(job.Title))
					{
						Console.WriteLine($"  - {job.Title} at {job.Company}");

						// Fetch full details if we have a URL
						if (!string.IsNullOrEmpty(job.SourceUrl))
						{
							try
							{
								await FetchJobDetailsAsync(job).ConfigureAwait(false);
							}
							catch (Exception ex)
							{
								Console.WriteLine($"    Warning: Could not fetch details - {ex.Message}");
							}
						}

						Job existing =
							await jobRepository.GetBySourceIdAsync(
								SourceName, job.SourceJobId).ConfigureAwait(false);

						if (existing == null)
						{
							await AddOrUpdateJobAsync(job).ConfigureAwait(false);
							result.JobsAdded++;
						}
						else
						{
							await AddOrUpdateJobAsync(job).ConfigureAwait(false);
							result.JobsUpdated++;
						}

						result.Jobs.Add(job);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"  Error parsing job: {ex.Message}");
				}
			}

			result.JobsFound = result.Jobs.Count;
			result.Success = true;
			Console.WriteLine($"✓ Scraping complete: {result.JobsAdded} added, {result.JobsUpdated} updated");
		}
		catch (Exception ex)
		{
			result.Success = false;
			result.ErrorMessage = ex.Message;
			Console.WriteLine($"❌ Scraping failed: {ex.Message}");
		}
		finally
		{
			driver = null!;
		}

		stopwatch.Stop();
		result.Duration = stopwatch.Elapsed;
		return result;
	}

	/// <inheritdoc/>
	protected override async Task<bool> LoginAsync(ScraperCredential credential)
	{
		// TokyoDev doesn't require login for public job listings
		return await Task.FromResult(true).ConfigureAwait(false);
	}

	private static string CleanText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}

		return HtmlEntity.DeEntitize(text)
			.Trim()
			.Replace("\n", " ", StringComparison.InvariantCultureIgnoreCase)
			.Replace("\r", string.Empty, StringComparison.InvariantCultureIgnoreCase)
			.Replace("\t", " ", StringComparison.InvariantCultureIgnoreCase);
	}

	private static void ConfigureDriver(IWebDriver driver)
	{
		driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
		driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

		// Remove webdriver property
		IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
		jsExecutor.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
	}

	private static IWebDriver CreateDriver()
	{
		ChromeOptions options = new ChromeOptions();

		// Make it look more like a real browser
		options.AddArgument("--disable-blink-features=AutomationControlled");
		options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--no-sandbox");
		options.AddArgument("--disable-dev-shm-usage");
		options.AddArgument("--window-size=1920,1080");

		// Optional: Run headless (comment out to see the browser)
		// options.AddArgument("--headless=new");

		// Exclude automation flags
		options.AddExcludedArgument("enable-automation");
		options.AddAdditionalOption("useAutomationExtension", false);

		return new ChromeDriver(options);
	}

	private static List<HtmlNode> FindJobNodes(HtmlDocument doc)
	{
		// Try multiple selector strategies in order of likelihood
		Func<List<HtmlNode>>[] strategies = new Func<List<HtmlNode>>[]
		{
			// Strategy 1: Look for job-specific classes
			() => doc.DocumentNode.SelectNodes("//div[contains(@class, 'job-card')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//div[contains(@class, 'job-listing')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//article[contains(@class, 'job')]")?.ToList(),

			// Strategy 2: Look for links to job pages
			() => doc.DocumentNode.SelectNodes("//a[contains(@href, '/jobs/') and not(contains(@href, '/jobs?'))]")?.ToList(),

			// Strategy 3: Look for list items
			() => doc.DocumentNode.SelectNodes("//li[contains(@class, 'listing')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//li[.//a[contains(@href, '/jobs/')]]")?.ToList(),

			// Strategy 4: Generic card patterns
			() => doc.DocumentNode.SelectNodes("//div[contains(@class, 'card')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//article")?.ToList(),

			// Strategy 5: Data attributes
			() => doc.DocumentNode.SelectNodes("//*[@data-job-id]")?.ToList(),
		};

		foreach (Func<List<HtmlNode>> strategy in strategies)
		{
			try
			{
				List<HtmlNode> nodes = strategy();
				if (nodes != null && nodes.Any())
				{
					Console.WriteLine($"✓ Using selector strategy that found {nodes.Count} nodes");
					return nodes;
				}
			}
			catch
			{
			}
		}

		return null;
	}

	private static void ParseSalary(string salaryText, Job job)
	{
		MatchCollection numbers = Regex.Matches(salaryText, @"[\d,]+");

		if (numbers.Count >= 2)
		{
			if (decimal.TryParse(numbers[0].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out decimal min))
			{
				job.SalaryMin = min;
			}

			if (decimal.TryParse(numbers[1].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out decimal max))
			{
				job.SalaryMax = max;
			}
		}

		job.SalaryCurrency = salaryText.Contains("¥", StringComparison.InvariantCultureIgnoreCase) ||
			salaryText.ToLower(CultureInfo.InvariantCulture).Contains("jpy", StringComparison.InvariantCultureIgnoreCase) ? "JPY" : "USD";
	}

	private async Task FetchJobDetailsAsync(Job job)
	{
		driver.Navigate().GoToUrl(job.SourceUrl);
		Thread.Sleep(2000);

		string pageSource = driver.PageSource;
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(pageSource);

		// Extract description
		HtmlNode? descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]")
			?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content')]")
			?? doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'job-details')]");

		if (descNode != null)
		{
			job.Description = CleanText(descNode.InnerText);
		}

		// Extract salary
		HtmlNode? salaryNode = doc.DocumentNode.SelectSingleNode(
			"//*[contains(@class, 'salary')]");
		if (salaryNode != null)
		{
			ParseSalary(salaryNode.InnerText, job);
		}

		await Task.CompletedTask.ConfigureAwait(false);
	}

	private Job ParseJobNode(HtmlNode node)
	{
		Job job = new Job
		{
			Source = SourceName,
			DatePosted = DateTime.UtcNow,
			DateScraped = DateTime.UtcNow
		};

		// Extract job title - try multiple selectors
		HtmlNode? titleNode = node.SelectSingleNode(".//h2")
			?? node.SelectSingleNode(".//h3")
			?? node.SelectSingleNode(".//h4")
			?? node.SelectSingleNode(".//*[contains(@class, 'title')]");

		if (titleNode != null)
		{
			job.Title = CleanText(titleNode.InnerText);
		}
		else if (node.Name == "a")
		{
			job.Title = CleanText(node.InnerText);
		}

		// Extract company
		HtmlNode? companyNode = node.SelectSingleNode(".//*[contains(@class, 'company')]")
			?? node.SelectSingleNode(".//*[contains(text(), 'at ')]");

		if (companyNode != null)
		{
			job.Company = CleanText(companyNode.InnerText).Replace("at ", string.Empty, StringComparison.InvariantCultureIgnoreCase);
		}

		// Extract location
		HtmlNode? locationNode = node.SelectSingleNode(".//*[contains(@class, 'location')]")
			?? node.SelectSingleNode(".//*[contains(@class, 'remote')]");

		job.Location = locationNode != null ? CleanText(locationNode.InnerText) : "Tokyo, Japan";

		// Extract URL and ID
		HtmlNode? linkNode = node.SelectSingleNode(".//a[@href]") ?? (node.Name == "a" ? node : null);

		if (linkNode != null)
		{
			string href = linkNode.GetAttributeValue("href", string.Empty);
			job.SourceUrl = href.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? href : $"{BaseUrl}{href}";

			// Extract job ID from URL
			string[] urlParts = href.Trim('/').Split('/');
			job.SourceJobId = urlParts.LastOrDefault(p => !string.IsNullOrWhiteSpace(p) && p != "jobs")
				?? Guid.NewGuid().ToString();
		}
		else
		{
			job.SourceJobId = Guid.NewGuid().ToString();
		}

		return job;
	}

	private void ScrollPage()
	{
		// Scroll in steps to trigger lazy loading
		for (int i = 0; i < 3; i++)
		{
			((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 1000);");
			Thread.Sleep(1000);
		}

		// Scroll back to top
		((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
		Thread.Sleep(500);
	}
}
