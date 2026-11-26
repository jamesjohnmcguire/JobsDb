/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailReadStatus.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

/// <summary>
/// Describes reading an exported email, independently of job parsing.
/// </summary>
public enum EmailReadStatus
{
	/// <summary>An email was read; job recognition has not run.</summary>
	Read,


	/// <summary>A recognized export could not be read or validated.</summary>
	Failed
}
