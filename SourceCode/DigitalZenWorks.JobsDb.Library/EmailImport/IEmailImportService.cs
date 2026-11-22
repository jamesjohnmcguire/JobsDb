/////////////////////////////////////////////////////////////////////////////
// <copyright file="IEmailImportService.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>Processes exported emails and persists supported job listings.</summary>
public interface IEmailImportService
{
	/// <summary>Imports immediate directory contents, leaving inputs intact.</summary>
	/// <param name="directoryPath">The completed export batch directory.</param>
	/// <returns>One outcome per message or unsupported file.</returns>
	Task<IReadOnlyList<EmailImportResult>> ImportDirectoryAsync(
		string directoryPath);
}
