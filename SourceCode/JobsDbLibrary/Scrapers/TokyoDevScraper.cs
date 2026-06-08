/////////////////////////////////////////////////////////////////////////////
// <copyright file="TokyoDevScraperPrevious .cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Scrapers;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDbLibrary.Scrapers;

public class TokyoDevScraperPrevious : JobScraperBase
{
	private readonly HttpClient _httpClient;
	private const string BaseUrl = "https://www.tokyodev.com";
	private const string JobsUrl = "https://www.tokyodev.com/jobs";

	public TokyoDevScraperPrevious(
		IJobRepository jobRepository,
		ICredentialRepository credentialRepository)
		: base(jobRepository, credentialRepository, "TokyoDev")
	{
		_httpClient = new HttpClient();
		_httpClient.DefaultRequestHeaders.Add(
			"User-Agent",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
	}

	public override async Task<ScraperResult> ScrapeJobsAsync(SearchFilter filter = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ScraperResult result = new ScraperResult();

		try
		{
			// TokyoDev typically doesn't require login for basic job listings
			// But we can use credentials if they implement a login system later
			var credential = await GetCredentialsAsync().ConfigureAwait(false);

			if (credential != null && credential.IsActive)
			{
				await LoginAsync(credential).ConfigureAwait(false);
			}

			// Fetch the jobs page
			var jobsHtml = await _httpClient.GetStringAsync(JobsUrl).ConfigureAwait(false);
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(jobsHtml);

			// Parse job listings - adjust selectors based on actual HTML structure
			var jobNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'job-listing')]")
				?? doc.DocumentNode.SelectNodes("//article[contains(@class, 'job')]")
				?? doc.DocumentNode.SelectNodes("//div[@class='job']");

			if (jobNodes == null || !jobNodes.Any())
			{
				// Try alternative selectors
				jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/jobs/')]");
			}

			if (jobNodes != null)
			{
				foreach (var jobNode in jobNodes)
				{
					try
					{
						var job = await ParseJobNodeAsync(jobNode).ConfigureAwait(false);

						if (job != null && !string.IsNullOrEmpty(job.Title))
						{
							var existing = await JobRepository.GetBySourceIdAsync(
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
						// Log but continue with other jobs
						Console.WriteLine($"Error parsing job: {ex.Message}");
					}
				}
			}

			result.JobsFound = result.Jobs.Count;
			result.Success = true;
		}
		catch (Exception ex)
		{
			result.Success = false;
			result.ErrorMessage = ex.Message;
		}

		stopwatch.Stop();
		result.Duration = stopwatch.Elapsed;
		return result;
	}

	private async Task<Job> ParseJobNodeAsync(HtmlNode node)
	{
		Job job = new Job();

		// Extract job title
		var titleNode = node.SelectSingleNode(".//h2")
			?? node.SelectSingleNode(".//h3")
			?? node.SelectSingleNode(".//a[contains(@class, 'job-title')]");

		if (titleNode != null)
		{
			job.Title = HtmlEntity.DeEntitize(titleNode.InnerText.Trim());
		}

		// Extract company
		var companyNode = node.SelectSingleNode(".//span[contains(@class, 'company')]")
			?? node.SelectSingleNode(".//div[contains(@class, 'company')]");

		if (companyNode != null)
		{
			job.Company = HtmlEntity.DeEntitize(companyNode.InnerText.Trim());
		}

		// Extract location
		var locationNode = node.SelectSingleNode(".//span[contains(@class, 'location')]")
			?? node.SelectSingleNode(".//div[contains(@class, 'location')]");

		if (locationNode != null)
		{
			job.Location = HtmlEntity.DeEntitize(locationNode.InnerText.Trim());
		}

		// Extract job URL
		var linkNode = node.SelectSingleNode(".//a[@href]") ?? node;
		var href = linkNode.GetAttributeValue("href", string.Empty);

		if (!string.IsNullOrEmpty(href))
		{
			job.SourceUrl = href.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? href : $"{BaseUrl}{href}";

			// Extract job ID from URL
			var urlParts = href.Split('/');
			job.SourceJobId = urlParts.LastOrDefault(p => !string.IsNullOrWhiteSpace(p)) ?? Guid.NewGuid().ToString();
		}
		else
		{
			job.SourceJobId = Guid.NewGuid().ToString();
		}

		// If we have a URL, fetch the full job details
		if (!string.IsNullOrEmpty(job.SourceUrl))
		{
			try
			{
				await FetchJobDetailsAsync(job).ConfigureAwait(false);
			}
			catch
			{
				// Continue with basic info if details fetch fails
			}
		}

		job.DatePosted = DateTime.UtcNow; // Default to today if not found
		job.Source = sourceName;

		return job;
	}

	private async Task FetchJobDetailsAsync(Job job)
	{
		var detailsHtml = await _httpClient.GetStringAsync(job.SourceUrl).ConfigureAwait(false);
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(detailsHtml);

		// Extract description
		var descNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]")
			?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'job-content')]")
			?? doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'description')]");

		if (descNode != null)
		{
			job.Description = HtmlEntity.DeEntitize(descNode.InnerText.Trim());
		}

		// Extract requirements
		var reqNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'requirements')]")
			?? doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'requirements')]");

		if (reqNode != null)
		{
			job.Requirements = HtmlEntity.DeEntitize(reqNode.InnerText.Trim());
		}

		// Extract salary if available
		var salaryNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'salary')]")
			?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'salary')]");

		if (salaryNode != null)
		{
			var salaryText = salaryNode.InnerText.Trim();
			ParseSalary(salaryText, job);
		}

		// Extract job type (Full-time, Part-time, etc.)
		var typeNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'job-type')]");
		if (typeNode != null)
		{
			job.JobType = HtmlEntity.DeEntitize(typeNode.InnerText.Trim());
		}

		// Extract remote type
		var remoteNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'remote')]");
		if (remoteNode != null)
		{
			job.RemoteType = HtmlEntity.DeEntitize(remoteNode.InnerText.Trim());
		}
	}

	private static void ParseSalary(string salaryText, Job job)
	{
		// Parse salary strings like "¥5,000,000 - ¥8,000,000" or "$50,000 - $80,000"
		var numbers = System.Text.RegularExpressions.Regex.Matches(salaryText, @"[\d,]+");

		if (numbers.Count >= 2)
		{
			if (decimal.TryParse(numbers[0].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out var min))
				job.SalaryMin = min;

			if (decimal.TryParse(numbers[1].Value.Replace(",", string.Empty, StringComparison.InvariantCultureIgnoreCase), out var max))
				job.SalaryMax = max;
		}

		// Determine currency
		if (salaryText.Contains("¥", StringComparison.InvariantCultureIgnoreCase) ||
			salaryText.ToLower(CultureInfo.InvariantCulture).Contains("jpy", StringComparison.InvariantCultureIgnoreCase))
			job.SalaryCurrency = "JPY";
		else if (salaryText.Contains("$", StringComparison.InvariantCultureIgnoreCase) ||
			salaryText.ToLower(CultureInfo.InvariantCulture).Contains("usd", StringComparison.InvariantCultureIgnoreCase))
			job.SalaryCurrency = "USD";
	}

	protected override async Task<bool> LoginAsync(ScraperCredential credential)
	{
		// TokyoDev typically doesn't require login for job listings
		// Implement if they add authentication in the future
		return await Task.FromResult(true).ConfigureAwait(false);
	}
}
