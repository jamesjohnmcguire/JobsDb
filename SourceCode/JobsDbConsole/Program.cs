namespace JobsDbConsole;

using JobsDbLibrary.Scrapers;
using System;
using System.Threading.Tasks;

internal class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("Hello, World!");
		var tester = new ScraperTester();

		// Analyze TokyoDev structure
		await tester.TestTokyoDev();

		// Or test a specific selector
		// await tester.TestSelector(
		//     "https://www.tokyodev.com/jobs", 
		//     "//div[contains(@class, 'job-card')]"
		// );
	}
}
