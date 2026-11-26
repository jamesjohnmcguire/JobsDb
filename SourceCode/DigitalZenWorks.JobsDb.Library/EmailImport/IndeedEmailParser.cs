/////////////////////////////////////////////////////////////////////////////
// <copyright file="IndeedEmailParser.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Utilities;
using HtmlAgilityPack;

/// <summary>
/// Extracts job cards from Indeed job-alert digests.
/// </summary>
internal sealed class IndeedEmailParser : IEmailJobParser
{
	/// <summary>Sender address for Indeed job alerts.</summary>
	internal const string JobAlertSender = "donotreply@jobalert.indeed.com";

	/// <inheritdoc/>
	public EmailParseResult Parse(Email email)
	{
		ArgumentNullException.ThrowIfNull(email);
		if (!email.SenderAddress.Equals(
			JobAlertSender, StringComparison.OrdinalIgnoreCase) ||
			string.IsNullOrWhiteSpace(email.HtmlBody))
		{
			return new EmailParseResult(
				EmailParseStatus.Ignored,
				false,
				Array.Empty<EmailJobCandidate>(),
				new[] { "No supported Indeed job-alert body was found." });
		}

		return ParseCards(email.HtmlBody, email.SentOn);
	}

	private static EmailParseResult ParseCards(string html, DateTime sentOn)
	{
		HtmlDocument document = new HtmlDocument();
		document.LoadHtml(html);
		List<EmailJobCandidate> candidates = new List<EmailJobCandidate>();
		HashSet<string> identifiers =
			new HashSet<string>(StringComparer.Ordinal);
		int unreadCards = 0;

		foreach (HtmlNode link in FindTitleLinks(document))
		{
			EmailJobCandidate? candidate = ReadCandidate(link, sentOn);
			if (candidate == null)
			{
				unreadCards++;
			}
			else if (identifiers.Add(candidate.SourceJobId))
			{
				candidates.Add(candidate);
			}
		}

		return CreateResult(candidates, unreadCards);
	}

	private static IEnumerable<HtmlNode> FindTitleLinks(HtmlDocument document)
	{
		return document.DocumentNode.Descendants("h2")
			.SelectMany(heading => heading.Elements("a"))
			.Where(link => link.GetClasses().Contains("strong-text-link"));
	}

	private static EmailJobCandidate? ReadCandidate(
		HtmlNode link, DateTime sentOn)
	{
		string? identifier = ReadPostingIdentifier(link);
		string title = HtmlText.Normalize(link.InnerText);
		HtmlNode? card = link.Ancestors("table").FirstOrDefault();
		if (identifier == null || title.Length == 0 || card == null)
		{
			return null;
		}

		HtmlNode[] details = card.Descendants("p").Take(2).ToArray();
		string? company = ReadDetail(details, 0);
		string? location = ReadDetail(details, 1);
		Uri postingUrl = new Uri(
			"https://www.indeed.com/viewjob?jk=" + identifier);
		return new EmailJobCandidate(
			"Indeed",
			identifier,
			postingUrl,
			title,
			company,
			location,
			sentOn
			, true);
	}

	private static string? ReadPostingIdentifier(HtmlNode link)
	{
		string target = HtmlEntity.DeEntitize(
			link.GetAttributeValue("href", string.Empty));
		if (!Uri.TryCreate(target, UriKind.Absolute, out Uri? address) ||
			address.Scheme != Uri.UriSchemeHttps || !address.IsDefaultPort ||
			address.UserInfo.Length != 0 ||
			!address.Host.Equals("www.indeed.com", StringComparison.Ordinal) ||
			!address.AbsolutePath.Equals(
				"/rc/clk/dl", StringComparison.Ordinal))
		{
			return null;
		}

		string[]? keys = HttpUtility.ParseQueryString(address.Query)
			.GetValues("jk");
		if (keys == null || keys.Length != 1 ||
			keys[0].Length != 16 || !keys[0].All(Uri.IsHexDigit))
		{
			return null;
		}

		return keys[0].ToLowerInvariant();
	}

	private static string? ReadDetail(HtmlNode[] details, int index)
	{
		if (index >= details.Length)
		{
			return null;
		}

		string value = HtmlText.Normalize(details[index].InnerText);
		return value.Length == 0 ? null : value;
	}

	private static EmailParseResult CreateResult(
		List<EmailJobCandidate> candidates, int unreadCards)
	{
		if (unreadCards > 0)
		{
			return new EmailParseResult(
				EmailParseStatus.NeedsReview,
				true,
				candidates,
				new[]
				{
					$"Could not read {unreadCards} Indeed job alert cards."
				});
		}

		if (candidates.Count == 0)
		{
			return new EmailParseResult(
				EmailParseStatus.Ignored,
				false,
				candidates,
				new[] { "No Indeed job alert cards were found." });
		}

		return new EmailParseResult(
			EmailParseStatus.Parsed, true, candidates, Array.Empty<string>());
	}
}
