/////////////////////////////////////////////////////////////////////////////
// <copyright file="Email.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Models;

using System;

/// <summary>
/// An email supplied by an acquisition adapter for library processing.
/// </summary>
public class Email
{
	/// <summary>
	/// Gets or sets the sent time. Preserve an unspecified kind when an
	/// exported timestamp does not identify its timezone.
	/// </summary>
	public DateTime SentOn { get; set; }

	/// <summary>
	/// Gets or sets the sender's display name.
	/// </summary>
	public string SenderName { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the sender's email address.
	/// </summary>
	public string SenderAddress { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the original HTML body, when available.
	/// </summary>
	public string? HtmlBody { get; set; }

	/// <summary>
	/// Gets or sets the original plain-text body, when available.
	/// </summary>
	public string? TextBody { get; set; }

	/// <summary>
	/// Gets or sets whether this email contains a collection of job listings.
	/// This flag does not indicate whether processing has succeeded.
	/// </summary>
	public bool IsAggregate { get; set; }
}
