/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperCredentialConfiguration.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core configuration for the ScraperCredential entity.
/// Auto-discovered and applied via ModelBuilder.ApplyConfigurationsFromAssembly.
/// </summary>
public class ScraperCredentialConfiguration : IEntityTypeConfiguration<ScraperCredential>
{
	public void Configure(EntityTypeBuilder<ScraperCredential> entity)
	{
		entity.HasIndex(e => e.Source).IsUnique();

		entity.Property(e => e.IsActive)
			.HasDefaultValue(true);

		entity.Property(e => e.Source)
			.IsRequired()
			.HasMaxLength(50);

		entity.Property(e => e.Username)
			.IsRequired()
			.HasMaxLength(200);

		entity.Property(e => e.EncryptedPassword)
			.IsRequired();
	}
}
