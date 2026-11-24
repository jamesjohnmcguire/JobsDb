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
	/// <summary>
	/// The source name used to store LinkedIn listings.
	/// </summary>
	public const string SourceName = "LinkedIn";

	/// <summary>
	/// Reads a numeric posting identifier from a supported LinkedIn job URL.
	/// </summary>
	/// <param name="address">
	/// The absolute or root-relative posting address.
	/// </param>
	/// <returns>
	/// The posting identifier, or null if the address is unsupported.
	/// </returns>
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

	/// <summary>
	/// Creates a LinkedIn posting URL from its numeric identifier.
	/// </summary>
	/// <param name="identifier">The numeric posting identifier.</param>
	/// <returns>The canonical HTTPS address of the listing.</returns>
	/// <exception cref="ArgumentException">
	/// The identifier is empty or contains a non-digit.
	/// </exception>
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

	/// <summary>
	/// Finds the first supported posting identifier, then checks the fallback.
	/// </summary>
	/// <param name="addresses">
	/// The candidate posting addresses in priority order.
	/// </param>
	/// <param name="fallbackIdentifier">
	/// The raw identifier to check if no address matches.
	/// </param>
	/// <returns>
	/// A validated posting identifier, or null if no candidate matches.
	/// </returns>
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

	/// <summary>
	/// Tests whether a posting identifier consists of ASCII digits.
	/// </summary>
	/// <param name="identifier">The identifier to validate.</param>
	/// <returns>
	/// True if the identifier contains one or more digits, otherwise false.
	/// </returns>
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
