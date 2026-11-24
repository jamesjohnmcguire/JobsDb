/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialConfig.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library;

/// <summary>
/// Holds the JSON credential settings for one source.
/// </summary>
public class CredentialConfig
{
	/// <summary>
	/// Gets or sets the job source identifier.
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// Gets or sets the account username.
	/// </summary>
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the plaintext password stored in local configuration.
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the credential is enabled.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Gets or sets notes about the credential.
	/// </summary>
	public string Notes { get; set; }
}
