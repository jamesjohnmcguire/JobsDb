/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperLogConfiguration.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
