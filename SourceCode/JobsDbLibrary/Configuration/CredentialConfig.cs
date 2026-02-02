/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialConfig.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core.Configuration;

public class CredentialConfig
{
	public string Source { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public bool IsActive { get; set; }
	public string Notes { get; set; }
}
