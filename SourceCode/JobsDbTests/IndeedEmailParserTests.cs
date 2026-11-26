/////////////////////////////////////////////////////////////////////////////
// <copyright file="IndeedEmailParserTests.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using DigitalZenWorks.JobsDb.Library.Models;
using NUnit.Framework;

/// <summary>
/// Covers stable identities and partial Indeed job-alert parsing.
/// </summary>
[TestFixture]
internal sealed class IndeedEmailParserTests
{
	private const string OrganicLink =
		"https://www.indeed.com/rc/clk/dl?jk=0123456789ABCDEF&amp;from=alert";

	/// <summary>
	/// Checks field extraction, canonical links, and duplicate keys.
	/// </summary>
	[Test]
	public void Parse_ExtractsAndDeduplicatesOrganicCards()
	{
		string card = CreateCard(OrganicLink);
		Email email = CreateEmail(card + card);

		EmailParseResult result = new IndeedEmailParser().Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Parsed));
		Assert.That(result.Candidates, Has.Count.EqualTo(1));
		EmailJobCandidate candidate = result.Candidates[0];
		Assert.That(candidate.Source, Is.EqualTo("Indeed"));
		Assert.That(candidate.SourceJobId, Is.EqualTo("0123456789abcdef"));
		Assert.That(
			candidate.SourceUrl.AbsoluteUri,
			Is.EqualTo("https://www.indeed.com/viewjob?jk=0123456789abcdef"));
		Assert.That(candidate.Title, Is.EqualTo("Senior C# Engineer"));
		Assert.That(candidate.Company, Is.EqualTo("Tools & Systems"));
		Assert.That(candidate.Location, Is.EqualTo("Tokyo"));
		Assert.That(candidate.DatePosted, Is.EqualTo(email.SentOn));
		Assert.That(candidate.IsPostingDateEstimated, Is.True);
	}

	/// <summary>
	/// Keeps the identified listing when a second card has no reliable key.
	/// </summary>
	/// <param name="unresolvedLink">The unresolved job-card link.</param>
	[TestCase("https://www.indeed.com/pagead/clk/dl?ad=opaque")]
	[TestCase("https://indeed.com.example.org/rc/clk/dl?jk=0123456789abcdef")]
	[TestCase("https://www.indeed.com/rc/clk/dl?jk=0123456789abcdef&amp;jk=1")]
	public void Parse_UnresolvedCardRequiresReview(string unresolvedLink)
	{
		Email email = CreateEmail(
			CreateCard(OrganicLink) + CreateCard(unresolvedLink));

		EmailParseResult result = new IndeedEmailParser().Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.NeedsReview));
		Assert.That(result.Candidates, Has.Count.EqualTo(1));
	}

	private static Email CreateEmail(string html)
	{
		Email email = new Email();
		email.SenderAddress = IndeedEmailParser.JobAlertSender;
		email.SentOn = new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
		email.HtmlBody = html;
		return email;
	}

	private static string CreateCard(string href)
	{
		return "<table><tr><td><h2><a class='strong-text-link' href='" +
			href + "'>Senior&nbsp; C# Engineer</a></h2></td></tr>" +
			"<tr><td><table><tr><td><p>Tools &amp; Systems</p></td></tr>" +
			"<tr><td><p>Tokyo</p></td></tr></table></td></tr></table>";
	}
}
