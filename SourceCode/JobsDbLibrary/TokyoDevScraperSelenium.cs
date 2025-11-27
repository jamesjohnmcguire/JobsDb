using HtmlAgilityPack;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDbLibrary.Configuration;
using JobsDbLibrary.Scrapers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobsDb.Core.Scrapers
{
	/// <summary>
	/// TokyoDev scraper using Selenium WebDriver to bypass bot detection
	/// This behaves like a real browser and should work even with Cloudflare/bot protection
	/// </summary>
	public class TokyoDevScraperSelenium : JobScraperBase
	{
		private IWebDriver _driver;
		private readonly CookieManager _cookieManager;
		private readonly ConfigurationManager _configManager;
		private const string BaseUrl = "https://www.tokyodev.com";
		private const string JobsUrl = "https://www.tokyodev.com/jobs";

		public TokyoDevScraperSelenium(
			IJobRepository jobRepository,
			ICredentialRepository credentialRepository,
			CookieManager cookieManager = null,
			ConfigurationManager configManager = null) 
			: base(jobRepository, credentialRepository, "TokyoDev")
		{
			_cookieManager = cookieManager ?? new CookieManager();
			_configManager = configManager ?? new Configuration.ConfigurationManager();
		}

		public override async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
		{
			var stopwatch = Stopwatch.StartNew();
			var result = new ScraperResult();

			try
			{
				InitializeDriver();

				// Navigate to jobs page
				Console.WriteLine($"Navigating to {JobsUrl}...");
				_driver.Navigate().GoToUrl(JobsUrl);
                
				// Wait for page to load
				Thread.Sleep(3000);
                
				// Scroll down to trigger any lazy loading
				ScrollPage();

				// Get the rendered HTML
				var pageSource = _driver.PageSource;
                
				// Save for debugging
				System.IO.File.WriteAllText("tokyodev_scraped.html", pageSource);
				Console.WriteLine("✓ Page source saved to tokyodev_scraped.html");

				// Parse with HtmlAgilityPack
				var doc = new HtmlDocument();
				doc.LoadHtml(pageSource);

				// Try multiple selector strategies
				var jobNodes = FindJobNodes(doc);

				if (jobNodes == null || !jobNodes.Any())
				{
					Console.WriteLine("❌ No job nodes found. Check tokyodev_scraped.html");
					Console.WriteLine("Page title: " + _driver.Title);
					Console.WriteLine("Current URL: " + _driver.Url);
                    
					result.Success = false;
					result.ErrorMessage = "No job listings found on page";
					return result;
				}

				Console.WriteLine($"✓ Found {jobNodes.Count} potential job listings");

				foreach (var jobNode in jobNodes)
				{
					try
					{
						var job = ParseJobNode(jobNode);
                        
						if (job != null && !string.IsNullOrEmpty(job.Title))
						{
							Console.WriteLine($"  - {job.Title} at {job.Company}");
                            
							// Fetch full details if we have a URL
							if (!string.IsNullOrEmpty(job.SourceUrl))
							{
								try
								{
									await FetchJobDetailsAsync(job);
								}
								catch (Exception ex)
								{
									Console.WriteLine($"    Warning: Could not fetch details - {ex.Message}");
								}
							}

							var existing = await _jobRepository.GetBySourceIdAsync(_sourceName, job.SourceJobId);
                            
							if (existing == null)
							{
								await AddOrUpdateJobAsync(job);
								result.JobsAdded++;
							}
							else
							{
								await AddOrUpdateJobAsync(job);
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
				CleanupDriver();
			}

			stopwatch.Stop();
			result.Duration = stopwatch.Elapsed;
			return result;
		}

		private void InitializeDriver()
		{
			var options = new ChromeOptions();
            
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
            
			_driver = new ChromeDriver(options);
			_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
			_driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            
			// Remove webdriver property
			_driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
		}

		private void CleanupDriver()
		{
			try
			{
				_driver?.Quit();
				_driver?.Dispose();
			}
			catch { }
		}

		private void ScrollPage()
		{
			// Scroll in steps to trigger lazy loading
			for (int i = 0; i < 3; i++)
			{
				((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollBy(0, 1000);");
				Thread.Sleep(1000);
			}
            
			// Scroll back to top
			((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0);");
			Thread.Sleep(500);
		}

		private List<HtmlNode> FindJobNodes(HtmlDocument doc)
		{
			// Try multiple selector strategies in order of likelihood
			var strategies = new Func<List<HtmlNode>>[]
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

			foreach (var strategy in strategies)
			{
				try
				{
					var nodes = strategy();
					if (nodes != null && nodes.Any())
					{
						Console.WriteLine($"✓ Using selector strategy that found {nodes.Count} nodes");
						return nodes;
					}
				}
				catch { }
			}

			return null;
		}

		private Job ParseJobNode(HtmlNode node)
		{
			var job = new Job
			{
				Source = _sourceName,
				DatePosted = DateTime.UtcNow,
				DateScraped = DateTime.UtcNow
			};

			// Extract job title - try multiple selectors
			var titleNode = node.SelectSingleNode(".//h2") 
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
			var companyNode = node.SelectSingleNode(".//*[contains(@class, 'company')]")
				?? node.SelectSingleNode(".//*[contains(text(), 'at ')]");
            
			if (companyNode != null)
			{
				job.Company = CleanText(companyNode.InnerText).Replace("at ", "");
			}

			// Extract location
			var locationNode = node.SelectSingleNode(".//*[contains(@class, 'location')]")
				?? node.SelectSingleNode(".//*[contains(@class, 'remote')]");
            
			job.Location = locationNode != null ? CleanText(locationNode.InnerText) : "Tokyo, Japan";

			// Extract URL and ID
			var linkNode = node.SelectSingleNode(".//a[@href]") ?? (node.Name == "a" ? node : null);
            
			if (linkNode != null)
			{
				var href = linkNode.GetAttributeValue("href", string.Empty);
				job.SourceUrl = href.StartsWith("http") ? href : $"{BaseUrl}{href}";
                
				// Extract job ID from URL
				var urlParts = href.Trim('/').Split('/');
				job.SourceJobId = urlParts.LastOrDefault(p => !string.IsNullOrWhiteSpace(p) && p != "jobs") 
					?? Guid.NewGuid().ToString();
			}
			else
			{
				job.SourceJobId = Guid.NewGuid().ToString();
			}

			return job;
		}

		private async Task FetchJobDetailsAsync(Job job)
		{
			_driver.Navigate().GoToUrl(job.SourceUrl);
			Thread.Sleep(2000);
            
			var pageSource = _driver.PageSource;
			var doc = new HtmlDocument();
			doc.LoadHtml(pageSource);

			// Extract description
			var descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]")
				?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content')]")
				?? doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'job-details')]");
            
			if (descNode != null)
			{
				job.Description = CleanText(descNode.InnerText);
			}

			// Extract salary
			var salaryNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'salary')]");
			if (salaryNode != null)
			{
				ParseSalary(salaryNode.InnerText, job);
			}

			await Task.CompletedTask;
		}

		private void ParseSalary(string salaryText, Job job)
		{
			var numbers = System.Text.RegularExpressions.Regex.Matches(salaryText, @"[\d,]+");
            
			if (numbers.Count >= 2)
			{
				if (decimal.TryParse(numbers[0].Value.Replace(",", ""), out var min))
					job.SalaryMin = min;
                
				if (decimal.TryParse(numbers[1].Value.Replace(",", ""), out var max))
					job.SalaryMax = max;
			}

			job.SalaryCurrency = salaryText.Contains("¥") || salaryText.ToLower().Contains("jpy") ? "JPY" : "USD";
		}

		private string CleanText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;
            
			return HtmlEntity.DeEntitize(text)
				.Trim()
				.Replace("\n", " ")
				.Replace("\r", "")
				.Replace("\t", " ");
		}

		protected override async Task<bool> LoginAsync(ScraperCredential credential)
		{
			// TokyoDev doesn't require login for public job listings
			return await Task.FromResult(true);
		}
	}
}
