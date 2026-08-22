/////////////////////////////////////////////////////////////////////////////
// <copyright file="SearchFilterConfiguration.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalZenWorks.JobsDb.Library.Data;

/// <summary>
/// EF Core configuration for the SearchFilter entity.
/// Auto-discovered and applied via ModelBuilder.ApplyConfigurationsFromAssembly.
/// </summary>
public class SearchFilterConfiguration : IEntityTypeConfiguration<SearchFilter>
{
	public void Configure(EntityTypeBuilder<SearchFilter> entity)
	{
		entity.HasIndex(e => e.IsActive);
		entity.HasIndex(e => e.Name);

		entity.Property(e => e.IsActive)
			.HasDefaultValue(true);

		entity.Property(e => e.CreatedDate)
			.HasDefaultValueSql("datetime('now')");

		entity.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(100);

		entity.Property(e => e.Source)
			.HasMaxLength(50);

		entity.Property(e => e.Keywords)
			.HasMaxLength(500);

		entity.Property(e => e.Location)
			.HasMaxLength(200);
	}
}
