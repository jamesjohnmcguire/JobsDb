/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailImportReports.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Publishes the exporter entry IDs for a completed import batch.
/// </summary>
internal static class EmailImportReports
{
	/// <summary>
	/// Replaces the report files with the outcomes of the current batch.
	/// </summary>
	/// <param name="directoryPath">The export batch directory.</param>
	/// <param name="results">The batch's email import outcomes.</param>
	internal static void Write(
		string directoryPath, IReadOnlyList<EmailImportResult> results)
	{
		string suffix = "." + Guid.NewGuid() + ".tmp";
		string processedPath = Path.Combine(directoryPath, "processed.txt");
		string unprocessedPath = Path.Combine(directoryPath, "unprocessed.txt");
		string pendingProcessed = processedPath + suffix;
		string pendingUnprocessed = unprocessedPath + suffix;
		try
		{
			File.WriteAllLines(pendingProcessed, results
				.Where(result => result.Status == EmailImportStatus.Completed)
				.Select(result => result.SourceName));
			File.WriteAllLines(pendingUnprocessed, results
				.Where(result => result.Status != EmailImportStatus.Completed)
				.Select(result => result.SourceName));
			File.Move(pendingUnprocessed, unprocessedPath, overwrite: true);
			File.Move(pendingProcessed, processedPath, overwrite: true);
		}
		finally
		{
			File.Delete(pendingProcessed);
			File.Delete(pendingUnprocessed);
		}
	}
}
