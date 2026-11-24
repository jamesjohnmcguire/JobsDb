/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobPriority.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Models;

/// <summary>
/// Defines dashboard priority choices on the stored rating scale.
/// </summary>
public enum JobPriority
{
	/// <summary>
	/// A posting to review when time allows.
	/// </summary>
	Low = 1,

	/// <summary>
	/// A posting of normal interest.
	/// </summary>
	Medium = 3,

	/// <summary>
	/// A posting to review first.
	/// </summary>
	High = 5
}
