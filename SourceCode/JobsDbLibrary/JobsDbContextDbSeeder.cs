using JobsDb.Core.Models;
using JobsDb.Core.Scrapers;
using System;
using System.Linq;

namespace JobsDb.Core.Data
{
    /// <summary>
    /// Seeds reference/static data after migrations run.
    /// Safe to call every startup - checks before inserting (idempotent),
    /// similar in spirit to re-running a staticData.sql file.
    /// </summary>
    public static class DbSeeder
    {
        public static void Seed(JobsDbContext context)
        {
            SeedDefaultSearchFilters(context);
            context.SaveChanges();
        }

        private static void SeedDefaultSearchFilters(JobsDbContext context)
        {
            if (context.SearchFilters.Any(f => f.Name == "Tokyo Software Engineer"))
                return; // already seeded

            context.SearchFilters.Add(new SearchFilter
            {
                Name = "Tokyo Software Engineer",
                Keywords = "software engineer",
                Location = "Tokyo, Japan",
                Source = null, // all sources
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });
        }
    }
}
