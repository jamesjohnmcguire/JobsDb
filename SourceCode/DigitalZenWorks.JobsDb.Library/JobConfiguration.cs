using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// EF Core configuration for the Job entity.
/// Auto-discovered and applied via ModelBuilder.ApplyConfigurationsFromAssembly.
/// </summary>
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
	public void Configure(EntityTypeBuilder<Job> entity)
	{
		// Indexes for common query patterns
		entity.HasIndex(e => e.SourceJobId);
		entity.HasIndex(e => new { e.Source, e.SourceJobId }).IsUnique();
		entity.HasIndex(e => e.Status);
		entity.HasIndex(e => e.DatePosted);
		entity.HasIndex(e => e.IsArchived);
		entity.HasIndex(e => e.Company);
		entity.HasIndex(e => e.Location);

		// Enum stored as string for readability
		entity.Property(e => e.Status)
			.HasConversion<string>();

		// Default value for DateScraped
		entity.Property(e => e.DateScraped)
			.HasDefaultValueSql("datetime('now')");

		// Column constraints
		entity.Property(e => e.Title)
			.IsRequired()
			.HasMaxLength(500);

		entity.Property(e => e.Company)
			.IsRequired()
			.HasMaxLength(200);

		entity.Property(e => e.Source)
			.IsRequired()
			.HasMaxLength(50);

		entity.Property(e => e.SourceUrl)
			.IsRequired()
			.HasMaxLength(1000);

		entity.Property(e => e.SourceJobId)
			.HasMaxLength(200);

		entity.Property(e => e.Location)
			.HasMaxLength(200);

		entity.Property(e => e.JobType)
			.HasMaxLength(50);

		entity.Property(e => e.RemoteType)
			.HasMaxLength(50);

		entity.Property(e => e.SalaryCurrency)
			.HasMaxLength(10);
	}
}
