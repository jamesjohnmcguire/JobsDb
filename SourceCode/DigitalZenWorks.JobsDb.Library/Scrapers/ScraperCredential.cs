/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperCredential.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a stored credential for a job source.
/// </summary>
public class ScraperCredential
{
	/// <summary>
	/// Gets or sets the database identifier.
	/// </summary>
	[Key]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the source identifier.
	/// </summary>
	[Required]
	public string Source { get; set; } // "LinkedIn", "TokyoDev"

	/// <summary>
	/// Gets or sets the account username.
	/// </summary>
	[Required]
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the encrypted account password.
	/// </summary>
	[Required]
	public string EncryptedPassword { get; set; }

	/// <summary>
	/// Gets or sets the serialized browser cookie data.
	/// </summary>
	public string CookieData { get; set; }

	/// <summary>
	/// Gets or sets the last recorded use of the credential.
	/// </summary>
	public DateTime? LastUsed { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the credential is enabled.
	/// </summary>
	public bool IsActive { get; set; }
}
