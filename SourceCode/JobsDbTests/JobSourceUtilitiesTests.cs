/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobSourceUtilitiesTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using DigitalZenWorks.JobsDb.Library.Utilities;
using NUnit.Framework;

/// <summary>
/// Covers LinkedIn posting identity and HTML text normalization.
/// </summary>
[TestFixture]
internal sealed class JobSourceUtilitiesTests
{
	/// <summary>
	/// Checks that equivalent posting links resolve to one canonical address.
	/// </summary>
	/// <param name="address">The supported form of the posting link.</param>
	[TestCase("https://www.linkedin.com/jobs/view/123/?tracking=old")]
	[TestCase("https://www.linkedin.com/comm/jobs/view/123/?a=1&amp;b=2")]
	[TestCase("https://linkedin.com/jobs/view/123")]
	[TestCase("/jobs/view/123/?tracking=email")]
	public void LinkedInLinks_ShareCanonicalIdentity(string address)
	{
		string? identifier = LinkedInJobLinks.GetIdentifier(address);

		Assert.That(identifier, Is.EqualTo("123"));
		Assert.That(
			LinkedInJobLinks.GetCanonicalUrl(identifier!).AbsoluteUri,
			Is.EqualTo("https://www.linkedin.com/jobs/view/123/"));
	}

	/// <summary>
	/// Checks that nonnumeric or invented posting identifiers are rejected.
	/// </summary>
	/// <param name="value">The identifier to validate.</param>
	[TestCase("")]
	[TestCase("random-id")]
	[TestCase("１２３")]
	public void LinkedInLinks_RejectInventedOrInvalidIdentifiers(string value)
	{
		Assert.That(LinkedInJobLinks.IsValidIdentifier(value), Is.False);
		Assert.Throws<ArgumentException>(
			() => LinkedInJobLinks.GetCanonicalUrl(value));
	}

	/// <summary>
	/// Checks that a supported posting link takes precedence over an attribute.
	/// </summary>
	[Test]
	public void LinkedInLinks_UsePostingLinkBeforeValidatedAttribute()
	{
		string[] addresses =
		{
			"https://www.linkedin.com/company/example/",
			"/jobs/view/123/"
		};

		string? identifier = LinkedInJobLinks.FindIdentifier(addresses, "456");

		Assert.That(identifier, Is.EqualTo("123"));
	}

	/// <summary>
	/// Checks that fallback uses only a valid numeric identifier.
	/// </summary>
	/// <param name="attribute">The raw identifier attribute value.</param>
	/// <param name="expected">
	/// The accepted identifier or null for an invalid attribute.
	/// </param>
	[TestCase("456", "456")]
	[TestCase("unverified", null)]
	public void LinkedInLinks_ValidateAttributeFallback(
		string attribute, string? expected)
	{
		string[] addresses = { "https://www.linkedin.com/company/example/" };

		string? identifier =
			LinkedInJobLinks.FindIdentifier(addresses, attribute);

		Assert.That(identifier, Is.EqualTo(expected));
	}

	/// <summary>
	/// Checks that HTML entities and whitespace normalize to readable text.
	/// </summary>
	/// <param name="input">The extracted HTML text.</param>
	/// <param name="expected">The expected normalized text.</param>
	[TestCase(null, "")]
	[TestCase(" \r\n\t ", "")]
	[TestCase(" Senior&nbsp;\r\n C# &amp; 日本語 ", "Senior C# & 日本語")]
	public void HtmlText_DecodesEntitiesAndNormalizesWhitespace(
		string? input, string expected)
	{
		Assert.That(HtmlText.Normalize(input), Is.EqualTo(expected));
	}
}
