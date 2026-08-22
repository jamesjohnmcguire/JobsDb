/////////////////////////////////////////////////////////////////////////////
// <copyright file="ScraperCredential.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

using System;
using System.ComponentModel.DataAnnotations;

public class ScraperCredential
{
	[Key]
	public int Id { get; set; }

	[Required]
	public string Source { get; set; } // "LinkedIn", "TokyoDev"

	[Required]
	public string Username { get; set; }

	[Required]
	public string EncryptedPassword { get; set; }

	public string CookieData { get; set; }

	public DateTime? LastUsed { get; set; }

	public bool IsActive { get; set; }
}
