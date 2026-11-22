/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportStatus.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

/// <summary>Describes processing one exported message.</summary>
public enum EmailImportStatus
{
	/// <summary>Accepted candidates were saved or already matched stored jobs.</summary>
	Completed,

	/// <summary>The export or message template is unsupported.</summary>
	Ignored,

	/// <summary>Some candidates need additional information before saving.</summary>
	NeedsReview,

	/// <summary>The message could not be processed; its writes were rolled back.</summary>
	Failed
}
