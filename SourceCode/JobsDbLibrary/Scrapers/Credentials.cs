/////////////////////////////////////////////////////////////////////////////
// <copyright file="Credentials.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Scrapers;

public class Credentials
{
	public string Username { get; set; }

	public string Password { get; set; }

	public string CookieData { get; set; }

	public bool IsActive { get; set; }
}
