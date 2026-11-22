/////////////////////////////////////////////////////////////////////////////
// <copyright file="LinkedInJobLinks.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

/// <summary>
/// Shared LinkedIn posting identity rules for scraping and email ingestion.
/// </summary>
internal static class LinkedInJobLinks
{
	public const string SourceName = "LinkedIn";

	public static string? GetIdentifier(string address)
	{
		string decoded = WebUtility.HtmlDecode(address);
		if (decoded.StartsWith('/') &&
			!decoded.StartsWith("//", StringComparison.Ordinal))
		{
			decoded = "https://www.linkedin.com" + decoded;
		}

		if (!Uri.TryCreate(decoded, UriKind.Absolute, out Uri? destination))
		{
			return null;
		}

		if (!IsPostingHost(destination))
		{
			return null;
		}

		return ReadPostingPath(destination.AbsolutePath);
	}

	public static Uri GetCanonicalUrl(string identifier)
	{
		if (!IsValidIdentifier(identifier))
		{
			throw new ArgumentException(
				"A numeric LinkedIn posting identifier is required.",
				nameof(identifier));
		}

		string address = "https://www.linkedin.com/jobs/view/" +
			identifier + "/";
		return new Uri(address);
	}

	public static string? FindIdentifier(
		IEnumerable<string> addresses, string fallbackIdentifier)
	{
		foreach (string address in addresses)
		{
			string? identifier = GetIdentifier(address);
			if (identifier != null)
			{
				return identifier;
			}
		}

		if (IsValidIdentifier(fallbackIdentifier))
		{
			return fallbackIdentifier;
		}

		return null;
	}

	public static bool IsValidIdentifier(string identifier)
	{
		return !string.IsNullOrEmpty(identifier) &&
			identifier.All(char.IsAsciiDigit);
	}

	private static bool IsPostingHost(Uri destination)
	{
		bool supportedHost =
			destination.Host.Equals(
				"www.linkedin.com", StringComparison.OrdinalIgnoreCase) ||
			destination.Host.Equals(
				"linkedin.com", StringComparison.OrdinalIgnoreCase);
		return destination.Scheme == Uri.UriSchemeHttps &&
			destination.IsDefaultPort && destination.UserInfo.Length == 0 &&
			supportedHost;
	}

	private static string? ReadPostingPath(string path)
	{
		string[] segments = path.Trim('/').Split('/');
		if (segments.Length == 4 && segments[0] == "comm")
		{
			segments = segments.Skip(1).ToArray();
		}

		if (segments.Length != 3 ||
			segments[0] != "jobs" || segments[1] != "view")
		{
			return null;
		}

		if (!IsValidIdentifier(segments[2]))
		{
			return null;
		}

		return segments[2];
	}
}
