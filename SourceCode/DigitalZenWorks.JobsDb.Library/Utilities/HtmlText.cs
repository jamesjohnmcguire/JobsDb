/////////////////////////////////////////////////////////////////////////////
// <copyright file="HtmlText.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Utilities;

using System;
using HtmlAgilityPack;

/// <summary>
/// Shared plain-text cleanup for extracted HTML text.
/// </summary>
internal static class HtmlText
{
	public static string Normalize(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}

		string decoded = HtmlEntity.DeEntitize(text);
		string[] words = decoded.Split(
			(char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		return string.Join(" ", words);
	}
}
