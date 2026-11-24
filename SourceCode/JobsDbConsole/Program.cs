/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbConsole;

using System;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Runs the TokyoDev page inspection from the console.
/// </summary>
internal sealed class Program
{
	private static async Task Main(string[] args)
	{
		Console.WriteLine("Hello, World!");
		using ScraperTester tester = new ScraperTester();

		// Analyze TokyoDev structure
		await tester.TestTokyoDev().ConfigureAwait(false);

		// Or test a specific selector
		// await tester.TestSelector(
		//     "https://www.tokyodev.com/jobs",
		//     "//div[contains(@class, 'job-card')]"
		// );
	}
}
