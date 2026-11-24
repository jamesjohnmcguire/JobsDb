/////////////////////////////////////////////////////////////////////////////
// <copyright file="Credentials.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Scrapers;

/// <summary>
/// Holds account details and session data for a scraper.
/// </summary>
public class Credentials
{
	/// <summary>
	/// Gets or sets the account username.
	/// </summary>
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the account password.
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	/// Gets or sets the serialized browser cookie data.
	/// </summary>
	public string CookieData { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the credential is enabled.
	/// </summary>
	public bool IsActive { get; set; }
}
