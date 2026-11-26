/////////////////////////////////////////////////////////////////////////////
// <copyright file="LinkedInEmailParser.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.EmailImport;

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalZenWorks.JobsDb.Library.Models;
using DigitalZenWorks.JobsDb.Library.Utilities;
using HtmlAgilityPack;

/// <summary>
/// Extracts LinkedIn job-alert cards from supported HTML digests.
/// </summary>
public sealed class LinkedInEmailParser : IEmailJobParser
{
	/// <summary>Sender address for LinkedIn job alerts.</summary>
	internal const string JobAlertSender = "jobalerts-noreply@linkedin.com";

	/// <inheritdoc/>
	public EmailParseResult Parse(Email email)
	{
		ArgumentNullException.ThrowIfNull(email);

		if (!IsSupportedSender(email.SenderAddress))
		{
			return Ignore("Unsupported LinkedIn sender.");
		}

		if (string.IsNullOrWhiteSpace(email.HtmlBody))
		{
			return Ignore("LinkedIn HTML body is unavailable.");
		}

		return ParseHtml(email.HtmlBody, email.SentOn);
	}

	private static bool IsSupportedSender(string address)
	{
		return address.Equals(
			JobAlertSender,
			StringComparison.OrdinalIgnoreCase);
	}

	private static EmailParseResult Ignore(string reason)
	{
		return new EmailParseResult(
			EmailParseStatus.Ignored,
			false,
			Array.Empty<EmailJobCandidate>(),
			new List<string> { reason });
	}

	private static EmailParseResult ParseHtml(
		string htmlBody, DateTime sentOn)
	{
		HtmlDocument document = new HtmlDocument();
		document.LoadHtml(htmlBody);
		List<EmailJobCandidate> candidates = new List<EmailJobCandidate>();
		List<string> messages = new List<string>();
		bool skippedCard = CollectCandidates(
			document, sentOn, candidates, messages);
		if (candidates.Count == 0)
		{
			return Ignore("No supported LinkedIn job cards were found.");
		}

		EmailParseStatus status = EmailParseStatus.Parsed;
		if (skippedCard)
		{
			status = EmailParseStatus.NeedsReview;
		}

		return new EmailParseResult(status, true, candidates, messages);
	}

	private static bool CollectCandidates(
		HtmlDocument document,
		DateTime sentOn,
		List<EmailJobCandidate> candidates,
		List<string> messages)
	{
		HashSet<string> identifiers =
			new HashSet<string>(StringComparer.Ordinal);
		bool skippedCard = false;
		foreach (HtmlNode link in FindTitleLinks(document))
		{
			EmailJobCandidate? candidate = ReadCandidate(link, sentOn);
			if (candidate == null)
			{
				skippedCard = true;
				messages.Add("Skipped an unsupported or incomplete job card.");
				continue;
			}

			if (!identifiers.Add(candidate.SourceJobId))
			{
				messages.Add("Duplicate posting " + candidate.SourceJobId +
					" was ignored.");
				continue;
			}

			candidates.Add(candidate);
			ReportMissingDetails(candidate, messages);
		}

		return skippedCard;
	}

	private static IEnumerable<HtmlNode> FindTitleLinks(HtmlDocument document)
	{
		return document.DocumentNode.Descendants("a")
			.Where(link => link.GetClasses().Contains("font-bold"))
			.Where(link => link.GetAttributeValue("href", string.Empty)
				.Contains("/jobs/view/", StringComparison.Ordinal));
	}

	private static EmailJobCandidate? ReadCandidate(
		HtmlNode link, DateTime sentOn)
	{
		string? identifier = LinkedInJobLinks.GetIdentifier(
			link.GetAttributeValue("href", string.Empty));
		HtmlNode? card = link.Ancestors("table").FirstOrDefault();
		string title = HtmlText.Normalize(link.InnerText);
		if (identifier == null || card == null || title.Length == 0)
		{
			return null;
		}

		if (!IsSinglePostingCard(card, identifier))
		{
			return null;
		}

		string details = ReadDetails(card);
		string? company = ReadCompany(details);
		string? location = ReadLocation(details);
		return CreateCandidate(identifier, title, company, location, sentOn);
	}

	private static bool IsSinglePostingCard(HtmlNode card, string identifier)
	{
		foreach (HtmlNode link in card.Descendants("a"))
		{
			string? otherIdentifier = LinkedInJobLinks.GetIdentifier(
				link.GetAttributeValue("href", string.Empty));
			if (otherIdentifier != null && otherIdentifier != identifier)
			{
				return false;
			}
		}

		return true;
	}

	private static string ReadDetails(HtmlNode card)
	{
		HtmlNode? details = card.Descendants("p")
			.FirstOrDefault(node => node.GetClasses().Contains("line-clamp-1"));
		if (details == null)
		{
			return string.Empty;
		}

		return HtmlText.Normalize(details.InnerText);
	}

	private static string? ReadCompany(string details)
	{
		if (details.Length == 0)
		{
			return null;
		}

		int separator = details.IndexOf('·', StringComparison.Ordinal);
		if (separator < 0)
		{
			return details;
		}

		return NonEmpty(details.Substring(0, separator));
	}

	private static string? ReadLocation(string details)
	{
		int separator = details.IndexOf('·', StringComparison.Ordinal);
		if (separator < 0)
		{
			return null;
		}

		return NonEmpty(details.Substring(separator + 1));
	}

	private static string? NonEmpty(string value)
	{
		string trimmed = value.Trim();
		if (trimmed.Length == 0)
		{
			return null;
		}

		return trimmed;
	}

	private static EmailJobCandidate CreateCandidate(
		string identifier,
		string title,
		string? company,
		string? location,
		DateTime sentOn)
	{
		Uri destination = LinkedInJobLinks.GetCanonicalUrl(identifier);
		return new EmailJobCandidate(
			LinkedInJobLinks.SourceName,
			identifier,
			destination,
			title,
			company,
			location,
			sentOn,
			true);
	}

	private static void ReportMissingDetails(
		EmailJobCandidate candidate, List<string> messages)
	{
		if (candidate.Company == null)
		{
			messages.Add("Posting " + candidate.SourceJobId +
				": company is unavailable; retain for review.");
		}

		if (candidate.Location == null)
		{
			messages.Add("Posting " + candidate.SourceJobId +
				": location is unavailable.");
		}
	}
}
