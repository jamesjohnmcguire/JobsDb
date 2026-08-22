using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// EF Core configuration for the ScraperLog entity.
/// Auto-discovered and applied via ModelBuilder.ApplyConfigurationsFromAssembly.
/// </summary>
public class ScraperLogConfiguration : IEntityTypeConfiguration<ScraperLog>
{
	public void Configure(EntityTypeBuilder<ScraperLog> entity)
	{
		entity.HasIndex(e => e.Timestamp);
		entity.HasIndex(e => e.Source);
		entity.HasIndex(e => e.Success);

		entity.Property(e => e.Source)
			.IsRequired()
			.HasMaxLength(50);

		entity.Property(e => e.Timestamp)
			.HasDefaultValueSql("datetime('now')");
	}
}
