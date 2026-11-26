/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExportedEmailReader.cs" company="Digital Zen Works">
// Copyright c 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Reads exported EML messages without modifying the input files.
/// </summary>
public static class ExportedEmailReader
{
	/// <summary>
	/// Reads the EML messages in the immediate directory.
	/// </summary>
	/// <param name="directoryPath">The export batch directory.</param>
	/// <returns>One read outcome per exported entry ID.</returns>
	public static IReadOnlyList<EmailReadResult> ReadDirectory(
		string directoryPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
		string[] paths = Directory.GetFiles(directoryPath);
		Array.Sort(paths, StringComparer.Ordinal);
		return paths.Where(IsEml)
			.GroupBy(path => Path.GetFileNameWithoutExtension(path),
				StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
			.Select(ReadExport)
			.ToList();
	}

	private static bool IsEml(string path)
	{
		string extension = Path.GetExtension(path);
		return extension.Equals(".eml", StringComparison.OrdinalIgnoreCase);
	}

	private static EmailReadResult ReadExport(IGrouping<string, string> group)
	{
		using IEnumerator<string> paths = group.GetEnumerator();
		paths.MoveNext();
		string path = paths.Current;
		if (paths.MoveNext())
		{
			return new EmailReadResult(
				group.Key,
				EmailReadStatus.Failed,
				null,
				"Duplicate EML exports.");
		}

		return EmlEmailReader.Read(group.Key, path);
	}
}
