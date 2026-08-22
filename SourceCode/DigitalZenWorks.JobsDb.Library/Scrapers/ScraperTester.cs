/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperTester.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbLibrary.Scrapers;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

/// <summary>
/// Utility class to help debug and test scrapers by analyzing HTML structure
/// Run this first to understand the page structure before adjusting selectors.
/// </summary>
public class ScraperTester
{
	private readonly HttpClient httpClient;

	public ScraperTester()
	{
		httpClient = new HttpClient();
		httpClient.DefaultRequestHeaders.Add(
			"User-Agent",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
	}

	/// <summary>
	/// Fetches a page and saves it locally for inspection
	/// Also prints out potential selectors to use.
	/// </summary>
	public async Task AnalyzePageStructure(string url, string outputFile = "page_analysis.html")
	{
		Console.WriteLine($"Fetching: {url}");

		var html = await httpClient.GetStringAsync(url).ConfigureAwait(false);

		// Save HTML for manual inspection
		File.WriteAllText(outputFile, html);
		Console.WriteLine($"✓ HTML saved to: {outputFile}");
		Console.WriteLine("  Open this file in a text editor to see the structure");
		Console.WriteLine();

		// Parse and analyze
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(html);

		Console.WriteLine("=== STRUCTURE ANALYSIS ===");
		Console.WriteLine();

		// Look for common job listing patterns
		AnalyzeJobListings(doc);

		// Look for pagination
		AnalyzePagination(doc);

		// Look for job details
		AnalyzeJobDetails(doc);

		Console.WriteLine();
		Console.WriteLine("=== RECOMMENDATIONS ===");
		Console.WriteLine("1. Open the saved HTML file and search for job-related class names");
		Console.WriteLine("2. Look for repeated patterns (these are likely job listings)");
		Console.WriteLine("3. Update your scraper's XPath/CSS selectors based on findings");
		Console.WriteLine("4. Test with a small sample first before full scraping");
	}

	private static void AnalyzeJobListings(HtmlDocument doc)
	{
		Console.WriteLine("--- Job Listing Candidates ---");

		// Check for common patterns
		var patterns = new[]
		{
			("//div[contains(@class, 'job')]", "Divs with 'job' in class"),
			("//article", "Article elements"),
			("//li[contains(@class, 'listing')]", "List items with 'listing'"),
			("//a[contains(@href, '/jobs/')]", "Links to job pages"),
			("//div[contains(@class, 'card')]", "Divs with 'card' in class"),
			("//div[@data-job-id]", "Divs with job-id data attribute")
		};

		foreach (var (xpath, description) in patterns)
		{
			var nodes = doc.DocumentNode.SelectNodes(xpath);
			if (nodes != null && nodes.Any())
			{
				Console.WriteLine($"✓ Found {nodes.Count} nodes: {description}");
				Console.WriteLine($"  XPath: {xpath}");

				// Show a sample
				if (nodes.Count > 0)
				{
					var sample = nodes[0].OuterHtml;
					if (sample.Length > 300)
					{
						sample = sample.Substring(0, 300) + "...";
					}

					Console.WriteLine($"  Sample: {sample}");
				}

				Console.WriteLine();
			}
		}
	}

	private static void AnalyzePagination(HtmlDocument doc)
	{
		Console.WriteLine("--- Pagination Detection ---");

		var paginationPatterns = new[]
		{
			"//nav[contains(@class, 'pagination')]",
			"//div[contains(@class, 'pagination')]",
			"//a[contains(text(), 'Next')]",
			"//button[contains(text(), 'Load more')]"
		};

		foreach (var xpath in paginationPatterns)
		{
			var nodes = doc.DocumentNode.SelectNodes(xpath);
			if (nodes != null && nodes.Any())
			{
				Console.WriteLine($"✓ Found pagination: {xpath}");
				Console.WriteLine($"  Count: {nodes.Count}");
			}
		}

		Console.WriteLine();
	}

	private static void AnalyzeJobDetails(HtmlDocument doc)
	{
		Console.WriteLine("--- Job Detail Elements ---");

		var detailPatterns = new[]
		{
			("//h1", "Main headings (likely job titles)"),
			("//h2", "Secondary headings"),
			("//h3", "Tertiary headings"),
			("//div[contains(@class, 'description')]", "Description containers"),
			("//div[contains(@class, 'salary')]", "Salary information"),
			("//span[contains(@class, 'location')]", "Location tags"),
			("//div[contains(@class, 'company')]", "Company information")
		};

		foreach (var (xpath, description) in detailPatterns)
		{
			var nodes = doc.DocumentNode.SelectNodes(xpath);
			if (nodes != null)
			{
				Console.WriteLine($"  {nodes.Count} × {description}");
			}
		}

		Console.WriteLine();
	}

	/// <summary>
	/// Test a specific XPath selector and show what it returns.
	/// </summary>
	public async Task TestSelector(string url, string xpath, int maxResults = 5)
	{
		var html = await httpClient.GetStringAsync(url).ConfigureAwait(false);
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(html);

		var nodes = doc.DocumentNode.SelectNodes(xpath);

		if (nodes == null || !nodes.Any())
		{
			Console.WriteLine($"❌ No nodes found for: {xpath}");
			return;
		}

		Console.WriteLine($"✓ Found {nodes.Count} nodes for: {xpath}");
		Console.WriteLine();

		var count = 0;
		foreach (var node in nodes.Take(maxResults))
		{
			count++;
			Console.WriteLine($"--- Result {count} ---");
			Console.WriteLine($"Tag: {node.Name}");
			Console.WriteLine($"Text: {node.InnerText.Trim().Substring(0, Math.Min(100, node.InnerText.Trim().Length))}");

			if (node.Attributes.Any())
			{
				Console.WriteLine("Attributes:");
				foreach (var attr in node.Attributes.Take(5))
				{
					Console.WriteLine($"  {attr.Name} = {attr.Value}");
				}
			}

			Console.WriteLine();
		}
	}

	/// <summary>
	/// Quick test specifically for TokyoDev.
	/// </summary>
	public async Task TestTokyoDev()
	{
		Console.WriteLine("=== TOKYODEV STRUCTURE TEST ===");
		Console.WriteLine();

		await AnalyzePageStructure("https://www.tokyodev.com/jobs", "tokyodev_jobs.html").ConfigureAwait(false);

		Console.WriteLine();
		Console.WriteLine("=== NEXT STEPS ===");
		Console.WriteLine("1. Open tokyodev_jobs.html in your browser");
		Console.WriteLine("2. Open DevTools (F12) and inspect job listings");
		Console.WriteLine("3. Find the CSS class or structure that wraps each job");
		Console.WriteLine("4. Update TokyoDevScraper.cs with the correct selectors");
		Console.WriteLine("5. Run this test again to verify");
	}
}
