/////////////////////////////////////////////////////////////////////////////
// <copyright file="LinkedInScraper.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Scrapers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JobsDb.Core;
using JobsDb.Core.Configuration;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDb.Core.Scrapers;
using JobsDb.Core.Services;
using JobsDbLibrary.Scrapers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

/// <summary>
/// LinkedIn scraper using Selenium WebDriver for browser automation
/// Handles login and job search scraping with anti-detection measures.
/// </summary>
public class LinkedInScraper : JobScraperBase
{
	private readonly CookieManager cookieManager;
	private readonly ConfigurationManager configManager;
	private IWebDriver driver;
	private bool isLoggedIn;
	private const string LinkedInJobsUrl = "https://www.linkedin.com/jobs/search/";
	private const string LinkedInLoginUrl = "https://www.linkedin.com/login";
	private const string MasterPassword = "JobsDb2024!"; // Should match CredentialsWindow

	public LinkedInScraper(
		IJobRepository jobRepository,
		ICredentialRepository credentialRepository,
		CookieManager cookieManager = null,
		ConfigurationManager configManager = null)
		: base(jobRepository, credentialRepository, "LinkedIn")
	{
		this.cookieManager = cookieManager ?? new CookieManager();
		this.configManager = configManager ?? new Configuration.ConfigurationManager();
	}

	public override async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ScraperResult result = new ScraperResult();

		try
		{
			InitializeDriver();

			var credential = await GetCredentialsAsync().ConfigureAwait(false);

			if (credential == null || !credential.IsActive)
			{
				result.Success = false;
				result.ErrorMessage = "LinkedIn credentials not configured or inactive";
				Console.WriteLine("❌ " + result.ErrorMessage);
				return result;
			}

			// Login to LinkedIn
			Console.WriteLine("Logging into LinkedIn...");
			var loginSuccess = await LoginAsync(credential).ConfigureAwait(false);

			if (!loginSuccess)
			{
				result.Success = false;
				result.ErrorMessage = "Failed to login to LinkedIn";
				Console.WriteLine("❌ Login failed");
				return result;
			}

			Console.WriteLine("✓ Login successful");

			// Build search URL with filters
			var searchUrl = BuildSearchUrl(filter);
			Console.WriteLine($"Navigating to: {searchUrl}");
			driver.Navigate().GoToUrl(searchUrl);

			// Wait for jobs to load
			Thread.Sleep(4000);

			// Scroll to load more jobs (LinkedIn uses lazy loading)
			Console.WriteLine("Scrolling to load more jobs...");
			for (int i = 0; i < 3; i++)
			{
				((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
				Thread.Sleep(2000);
			}

			// Get page source and parse
			var pageSource = driver.PageSource;
			System.IO.File.WriteAllText("linkedin_scraped.html", pageSource);
			Console.WriteLine("✓ Page source saved to linkedin_scraped.html");

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(pageSource);

			// Parse job cards - LinkedIn frequently changes class names
			var jobCards = FindJobCards(doc);

			if (jobCards == null || !jobCards.Any())
			{
				Console.WriteLine("❌ No job cards found. Check linkedin_scraped.html");
				result.Success = false;
				result.ErrorMessage = "No job listings found on page";
				return result;
			}

			Console.WriteLine($"✓ Found {jobCards.Count} job listings");

			foreach (var card in jobCards)
			{
				try
				{
					var job = ParseJobCard(card);

					if (job != null && !string.IsNullOrEmpty(job.Title))
					{
						Console.WriteLine($"  - {job.Title} at {job.Company}");

						// Try to get full details by clicking on the job
						try
						{
							await FetchJobDetailsAsync(job).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"    Warning: Could not fetch details - {ex.Message}");
						}

						var existing = await jobRepository.GetBySourceIdAsync(sourceName, job.SourceJobId).ConfigureAwait(false);

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
					Console.WriteLine($"  Error parsing job card: {ex.Message}");
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
			CleanupDriver();
		}

		stopwatch.Stop();
		result.Duration = stopwatch.Elapsed;
		return result;
	}

	private void InitializeDriver()
	{
		ChromeOptions options = new ChromeOptions();

		// Anti-detection settings
		options.AddArgument("--disable-blink-features=AutomationControlled");
		options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--no-sandbox");
		options.AddArgument("--disable-dev-shm-usage");
		options.AddArgument("--window-size=1920,1080");

		// Optional: Run headless (comment out to see the browser)
		// options.AddArgument("--headless=new");
		options.AddExcludedArgument("enable-automation");
		options.AddAdditionalOption("useAutomationExtension", false);

		driver = new ChromeDriver(options);
		driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
		driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

		// Remove webdriver property
		IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
		jsExecutor.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
	}

	private void CleanupDriver()
	{
		try
		{
			driver?.Quit();
			driver?.Dispose();
		}
		catch
		{
		}
	}

	protected override async Task<bool> LoginAsync(ScraperCredential credential)
	{
		try
		{
			driver.Navigate().GoToUrl(LinkedInLoginUrl);
			Thread.Sleep(2000);

			// Find and fill username
			var usernameField = driver.FindElement(By.Id("username"));
			usernameField.Clear();

			// Type slowly to appear more human
			foreach (char c in credential.Username)
			{
				usernameField.SendKeys(c.ToString());
				Thread.Sleep(100);
			}

			// Find and fill password
			var passwordField = driver.FindElement(By.Id("password"));
			passwordField.Clear();

			var decryptedPassword = CredentialEncryption.Decrypt(
				credential.EncryptedPassword,
				MasterPassword);

			foreach (char c in decryptedPassword)
			{
				passwordField.SendKeys(c.ToString());
				Thread.Sleep(100);
			}

			// Click login button
			var loginButton = driver.FindElement(By.XPath("//button[@type='submit']"));
			loginButton.Click();

			// Wait for redirect
			Thread.Sleep(5000);

			// Check if we need to handle 2FA or verification
			if (driver.Url.Contains("checkpoint", StringComparison.InvariantCultureIgnoreCase) ||
				driver.Url.Contains("challenge", StringComparison.InvariantCultureIgnoreCase))
			{
				Console.WriteLine("⚠ LinkedIn verification required. Please complete manually...");
				Console.WriteLine("Waiting 60 seconds for manual verification...");
				Thread.Sleep(60000); // Wait for user to complete verification
			}

			// Check if login was successful
			isLoggedIn = driver.Url.Contains("feed", StringComparison.InvariantCultureIgnoreCase) ||
							driver.Url.Contains("jobs", StringComparison.InvariantCultureIgnoreCase) ||
							driver.Url.Contains("mynetwork", StringComparison.InvariantCultureIgnoreCase) ||
							!driver.Url.Contains("login", StringComparison.InvariantCultureIgnoreCase);

			if (isLoggedIn)
			{
				Console.WriteLine("✓ Successfully logged in");
				credential.LastUsed = DateTime.UtcNow;
				await credentialRepository.AddOrUpdateAsync(credential).ConfigureAwait(false);
			}
			else
			{
				Console.WriteLine("❌ Login verification failed");
			}

			return isLoggedIn;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ Login error: {ex.Message}");
			return false;
		}
	}

	private static string BuildSearchUrl(SearchFilter filter)
	{
		var url = LinkedInJobsUrl + "?";
		List<string> parameters = new List<string>();

		if (filter != null)
		{
			if (!string.IsNullOrEmpty(filter.Keywords))
			{
				parameters.Add($"keywords={Uri.EscapeDataString(filter.Keywords)}");
			}

			if (!string.IsNullOrEmpty(filter.Location))
			{
				parameters.Add($"location={Uri.EscapeDataString(filter.Location)}");
			}
		}
		else
		{
			// Default search for Tokyo/Japan developer jobs
			parameters.Add("keywords=software%20developer");
			parameters.Add("location=Tokyo%2C%20Japan");
		}

		parameters.Add("f_TPR=r86400"); // Past 24 hours
		parameters.Add("f_JT=F"); // Full-time
		parameters.Add("sortBy=DD"); // Sort by date (most recent first)

		return url + string.Join("&", parameters);
	}

	private static List<HtmlNode> FindJobCards(HtmlDocument doc)
	{
		// Try multiple selector strategies (LinkedIn changes these frequently)
		Func<List<HtmlNode>>[] strategies = new Func<List<HtmlNode>>[]
		{
			() => doc.DocumentNode.SelectNodes("//li[contains(@class, 'jobs-search-results__list-item')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//div[contains(@class, 'job-card-container')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//div[contains(@class, 'jobs-search-results__list-item')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//li[contains(@class, 'job-card')]")?.ToList(),
			() => doc.DocumentNode.SelectNodes("//div[@data-job-id]")?.ToList(),
		};

		foreach (var strategy in strategies)
		{
			try
			{
				var nodes = strategy();
				if (nodes != null && nodes.Any())
				{
					Console.WriteLine($"✓ Found job cards using selector strategy");
					return nodes;
				}
			}
			catch
			{
			}
		}

		return null;
	}

	private Job ParseJobCard(HtmlNode card)
	{
		Job job = new Job
		{
			Source = sourceName,
			DateScraped = DateTime.UtcNow,
			JobType = "Full-time"
		};

		// Extract job title
		var titleNode = card.SelectSingleNode(".//h3[contains(@class, 'job-card-list__title')]")
			?? card.SelectSingleNode(".//a[contains(@class, 'job-card-container__link')]")
			?? card.SelectSingleNode(".//span[contains(@class, 'job-card-container__title')]")
			?? card.SelectSingleNode(".//h3");

		if (titleNode != null)
		{
			job.Title = CleanText(titleNode.InnerText);
		}

		// Extract company
		var companyNode = card.SelectSingleNode(".//h4[contains(@class, 'job-card-container__company-name')]")
			?? card.SelectSingleNode(".//a[contains(@class, 'job-card-container__company-name')]")
			?? card.SelectSingleNode(".//span[contains(@class, 'job-card-container__company-name')]")
			?? card.SelectSingleNode(".//h4");

		if (companyNode != null)
		{
			job.Company = CleanText(companyNode.InnerText);
		}

		// Extract location
		var locationNode = card.SelectSingleNode(".//span[contains(@class, 'job-card-container__metadata-item')]")
			?? card.SelectSingleNode(".//*[contains(@class, 'location')]");

		if (locationNode != null)
		{
			job.Location = CleanText(locationNode.InnerText);
		}

		// Extract job URL and ID
		var linkNode = card.SelectSingleNode(".//a[@href]");
		if (linkNode != null)
		{
			var href = linkNode.GetAttributeValue("href", string.Empty);
			job.SourceUrl = href;

			// Extract job ID from URL (pattern: /jobs/view/1234567890)
			var jobIdMatch = System.Text.RegularExpressions.Regex.Match(href, @"/jobs/view/(\d+)");
			if (jobIdMatch.Success)
			{
				job.SourceJobId = jobIdMatch.Groups[1].Value;
			}
			else
			{
				job.SourceJobId = Guid.NewGuid().ToString();
			}
		}
		else
		{
			// Try data attribute
			var jobId = card.GetAttributeValue("data-job-id", string.Empty);
			if (!string.IsNullOrEmpty(jobId))
			{
				job.SourceJobId = jobId;
				job.SourceUrl = $"https://www.linkedin.com/jobs/view/{jobId}";
			}
		}

		// Extract posted date
		var dateNode = card.SelectSingleNode(".//time")
			?? card.SelectSingleNode(".//*[contains(@class, 'time')]");

		if (dateNode != null)
		{
			var dateText = dateNode.InnerText.Trim();
			job.DatePosted = ParseLinkedInDate(dateText);
		}
		else
		{
			job.DatePosted = DateTime.UtcNow;
		}

		return job;
	}

	private async Task FetchJobDetailsAsync(Job job)
	{
		if (string.IsNullOrEmpty(job.SourceUrl))
		{
			return;
		}

		try
		{
			driver.Navigate().GoToUrl(job.SourceUrl);
			Thread.Sleep(3000);

			// Scroll to load full description
			((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
			Thread.Sleep(1000);

			var pageSource = driver.PageSource;
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(pageSource);

			// Extract full description
			var descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'jobs-description__content')]")
				?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__text')]")
				?? doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'description')]");

			if (descNode != null)
			{
				job.Description = CleanText(descNode.InnerText);
			}

			// Extract job criteria (seniority, job type, etc.)
			var criteriaNodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'job-criteria__item')]");
			if (criteriaNodes != null)
			{
				foreach (var node in criteriaNodes)
				{
					var text = node.InnerText.ToLower(CultureInfo.InvariantCulture);

					if (text.Contains("employment type", StringComparison.InvariantCultureIgnoreCase))
					{
						var valueNode = node.SelectSingleNode(".//span[contains(@class, 'job-criteria__text')]");
						if (valueNode != null)
						{
							job.JobType = CleanText(valueNode.InnerText);
						}
					}
				}
			}

			// Try to extract salary if present
			var salaryNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'salary')]");
			if (salaryNode != null)
			{
				ParseSalary(salaryNode.InnerText, job);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching job details: {ex.Message}");
		}

		await Task.CompletedTask.ConfigureAwait(false);
	}

	private static void ParseSalary(string salaryText, Job job)
	{
		var numbers = System.Text.RegularExpressions.Regex.Matches(salaryText, @"[\d,]+");

		if (numbers.Count >= 2)
		{
			if (decimal.TryParse(numbers[0].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out var min))
			{
				job.SalaryMin = min;
			}

			if (decimal.TryParse(numbers[1].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out var max))
			{
				job.SalaryMax = max;
			}
		}

		if (salaryText.Contains("¥", StringComparison.InvariantCultureIgnoreCase) ||
			salaryText.ToLower(CultureInfo.InvariantCulture).Contains("jpy", StringComparison.InvariantCultureIgnoreCase))
		{
			job.SalaryCurrency = "JPY";
		}
		else if (salaryText.Contains("$", StringComparison.InvariantCultureIgnoreCase) ||
			salaryText.ToLower(CultureInfo.InvariantCulture).Contains("usd", StringComparison.InvariantCultureIgnoreCase))
		{
			job.SalaryCurrency = "USD";
		}
	}

	private static DateTime ParseLinkedInDate(string dateText)
	{
		var now = DateTime.UtcNow;
		dateText = dateText.ToLower(CultureInfo.InvariantCulture).Trim();

		if (dateText.Contains("hour", StringComparison.InvariantCultureIgnoreCase))
		{
			var hours = int.TryParse(System.Text.RegularExpressions.Regex.Match(dateText, @"\d+").Value, out var h) ? h : 1;
			return now.AddHours(-hours);
		}
		else if (dateText.Contains("day", StringComparison.InvariantCultureIgnoreCase))
		{
			var days = int.TryParse(System.Text.RegularExpressions.Regex.Match(dateText, @"\d+").Value, out var d) ? d : 1;
			return now.AddDays(-days);
		}
		else if (dateText.Contains("week", StringComparison.InvariantCultureIgnoreCase))
		{
			var weeks = int.TryParse(System.Text.RegularExpressions.Regex.Match(dateText, @"\d+").Value, out var w) ? w : 1;
			return now.AddDays(-weeks * 7);
		}
		else if (dateText.Contains("month", StringComparison.InvariantCultureIgnoreCase))
		{
			var months = int.TryParse(System.Text.RegularExpressions.Regex.Match(dateText, @"\d+").Value, out var m) ? m : 1;
			return now.AddMonths(-months);
		}

		return now;
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
			.Replace("\t", " ", StringComparison.InvariantCultureIgnoreCase)
			.Replace("  ", " ", StringComparison.InvariantCultureIgnoreCase);
	}
}
