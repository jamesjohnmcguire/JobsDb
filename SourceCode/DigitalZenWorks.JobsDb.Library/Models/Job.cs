/////////////////////////////////////////////////////////////////////////////
// <copyright file="Job.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Models;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a job listing and the user's application history for it.
/// </summary>
public class Job
{
	/// <summary>
	/// Gets or sets the database identifier.
	/// </summary>
	[Key]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the advertised job title.
	/// </summary>
	[Required]
	public string Title { get; set; }

	/// <summary>
	/// Gets or sets the hiring company.
	/// </summary>
	[Required]
	public string Company { get; set; }

	/// <summary>
	/// Gets or sets the advertised work location, when supplied.
	/// </summary>
	public string? Location { get; set; }

	/// <summary>
	/// Gets or sets the posting description, when available.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the advertised qualifications or requirements.
	/// </summary>
	public string? Requirements { get; set; }

	/// <summary>
	/// Gets or sets the name of the source that supplied the listing.
	/// </summary>
	[Required]
	public string Source { get; set; } // "LinkedIn", "TokyoDev", etc.

	/// <summary>
	/// Gets or sets the address of the original posting.
	/// </summary>
	[Required]
	public string SourceUrl { get; set; }

	/// <summary>
	/// Gets or sets the identifier of the posting within its source.
	/// </summary>
	public string SourceJobId { get; set; }

	/// <summary>
	/// Gets or sets the lower bound of the advertised salary.
	/// </summary>
	public decimal? SalaryMin { get; set; }

	/// <summary>
	/// Gets or sets the upper bound of the advertised salary.
	/// </summary>
	public decimal? SalaryMax { get; set; }

	/// <summary>
	/// Gets or sets the currency of the advertised salary.
	/// </summary>
	public string? SalaryCurrency { get; set; }

	/// <summary>
	/// Gets or sets the employment type, such as full-time or contract work.
	/// </summary>
	public string? JobType { get; set; } // Full-time, Part-time, Contract, etc.

	/// <summary>
	/// Gets or sets the work arrangement, such as on-site, remote, or hybrid.
	/// </summary>
	public string? RemoteType { get; set; } // On-site, Remote, Hybrid

	/// <summary>
	/// Gets or sets the supplied or estimated posting date.
	/// </summary>
	public DateTime DatePosted { get; set; }

	/// <summary>
	/// Gets or sets the time the listing was last saved by acquisition.
	/// </summary>
	public DateTime DateScraped { get; set; }

	/// <summary>
	/// Gets or sets the application date, when recorded.
	/// </summary>
	public DateTime? DateApplied { get; set; }

	/// <summary>
	/// Gets or sets the user's application status.
	/// </summary>
	public ApplicationStatus Status { get; set; }

	/// <summary>
	/// Gets or sets the user's optional priority rating.
	/// </summary>
	public int? Priority { get; set; } // 1-5 rating

	/// <summary>
	/// Gets or sets the user's notes about the listing.
	/// </summary>
	public string? Notes { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the listing is archived.
	/// </summary>
	public bool IsArchived { get; set; }
}
