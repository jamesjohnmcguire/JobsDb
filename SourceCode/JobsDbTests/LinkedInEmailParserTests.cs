/////////////////////////////////////////////////////////////////////////////
// <copyright file="LinkedInEmailParserTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using DigitalZenWorks.JobsDb.Library.Models;
using NUnit.Framework;

[TestFixture]
internal sealed class LinkedInEmailParserTests
{
	[TestCase("jobalerts-noreply@linkedin.com")]
	[TestCase("JOBALERTS-NOREPLY@LINKEDIN.COM")]
	public void Parse_ExtractsCardAndPreservesOriginalEmail(string sender)
	{
		string card = CreateCard(
			"https://www.linkedin.com/comm/jobs/view/123/?trk=email&amp;x=1",
			" Senior&nbsp; C# &amp; 日本語 ",
			" Example &amp; Co &middot; 東京 (Remote) ");
		Email email = CreateEmail(card);
		email.SenderAddress = sender;

		EmailParseResult result = new LinkedInEmailParser().Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Parsed));
		Assert.That(result.IsAggregate, Is.True);
		Assert.That(result.Candidates, Has.Count.EqualTo(1));
		EmailJobCandidate candidate = result.Candidates[0];
		Assert.That(candidate.Source, Is.EqualTo("LinkedIn"));
		Assert.That(candidate.SourceJobId, Is.EqualTo("123"));
		Assert.That(
			candidate.SourceUrl.AbsoluteUri,
			Is.EqualTo("https://www.linkedin.com/jobs/view/123/"));
		Assert.That(candidate.Title, Is.EqualTo("Senior C# & 日本語"));
		Assert.That(candidate.Company, Is.EqualTo("Example & Co"));
		Assert.That(candidate.Location, Is.EqualTo("東京 (Remote)"));
		Assert.That(candidate.DatePosted, Is.EqualTo(email.SentOn));
		Assert.That(candidate.IsPostingDateEstimated, Is.True);
		Assert.That(result.Messages, Is.Empty);
		Assert.That(email.HtmlBody, Is.EqualTo(card));
		Assert.That(email.TextBody, Is.EqualTo("Original text"));
		Assert.That(email.IsAggregate, Is.False);
	}

	[Test]
	public void Parse_DeduplicatesPostingsAndIgnoresLogoLinks()
	{
		string card = CreateCard(
			"https://www.linkedin.com/comm/jobs/view/123/",
			"Developer",
			"Company &middot; Remote");
		Email email = CreateEmail(card + card);

		EmailParseResult result = new LinkedInEmailParser().Parse(email);

		Assert.That(result.Candidates, Has.Count.EqualTo(1));
		Assert.That(result.Messages, Has.Count.EqualTo(1));
		Assert.That(result.Messages[0], Does.Contain("Duplicate posting 123"));
	}

	[Test]
	public void Parse_KeepsCardDetailsSeparate()
	{
		string first = CreateCard(
			"https://www.linkedin.com/jobs/view/123/",
			"Developer",
			"First Company &middot; Tokyo");
		string second = CreateCard(
			"https://www.linkedin.com/jobs/view/456/",
			"Engineer",
			"Second Company &middot; New York");

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(first + second));

		Assert.That(result.Candidates, Has.Count.EqualTo(2));
		Assert.That(result.Candidates[0].Company, Is.EqualTo("First Company"));
		Assert.That(result.Candidates[1].Company, Is.EqualTo("Second Company"));
		Assert.That(result.Candidates[1].Location, Is.EqualTo("New York"));
	}

	[TestCase("https://linkedin.com.example.org/jobs/view/123/")]
	[TestCase("https://example.org/jobs/view/123/")]
	[TestCase("http://www.linkedin.com/jobs/view/123/")]
	[TestCase("https://www.linkedin.com:8443/jobs/view/123/")]
	[TestCase("https://name@www.linkedin.com/jobs/view/123/")]
	[TestCase("https://www.linkedin.com/jobs/view/not-a-number/")]
	[TestCase("https://www.linkedin.com/jobs/view/123/other")]
	public void Parse_IgnoresUnsupportedDestinations(string address)
	{
		string card = CreateCard(address, "Developer", "Company");

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(card));

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.Candidates, Is.Empty);
		Assert.That(result.Messages, Is.Not.Empty);
	}

	[TestCase(null)]
	[TestCase("")]
	public void Parse_TextOnlyEmailHasAnExplicitIgnoredOutcome(string? html)
	{
		Email email = CreateEmail(html);

		EmailParseResult result = new LinkedInEmailParser().Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.Messages[0], Does.Contain("HTML body"));
	}

	[Test]
	public void Parse_DoesNotClaimOtherProviders()
	{
		Email email = CreateEmail(CreateCard(
			"https://www.linkedin.com/jobs/view/123/",
			"Developer",
			"Company"));
		email.SenderAddress = "jobs@dice.com";

		EmailParseResult result = new LinkedInEmailParser().Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.IsAggregate, Is.False);
		Assert.That(result.Candidates, Is.Empty);
	}

	[Test]
	public void Parse_MissingDetailsRemainCandidatesForReview()
	{
		string card = CreateCard(
			"https://www.linkedin.com/jobs/view/123/",
			"Developer",
			null);

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(card));

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Parsed));
		Assert.That(result.Candidates[0].Company, Is.Null);
		Assert.That(result.Candidates[0].Location, Is.Null);
		Assert.That(result.Messages, Has.Count.EqualTo(2));
	}

	[Test]
	public void Parse_DoesNotBorrowDetailsFromAnotherCard()
	{
		string missing = CreateCard(
			"https://www.linkedin.com/jobs/view/123/",
			"Developer",
			null);
		string complete = CreateCard(
			"https://www.linkedin.com/jobs/view/456/",
			"Engineer",
			"Known Company &middot; Tokyo");

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(missing + complete));

		Assert.That(result.Candidates[0].Company, Is.Null);
		Assert.That(result.Candidates[1].Company, Is.EqualTo("Known Company"));
	}

	[Test]
	public void Parse_ChangedTemplateIsIgnoredWithReason()
	{
		string html = CreateCard(
			"https://www.linkedin.com/jobs/view/123/",
			"Developer",
			"Company");
		html = html.Replace(
			"font-bold",
			"unknown-style",
			StringComparison.Ordinal);

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(html));

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.Messages, Is.Not.Empty);
	}

	[Test]
	public void Parse_AmbiguousSharedTableIsIgnored()
	{
		string html = "<table><tr><td>" +
			CreateTitle("https://www.linkedin.com/jobs/view/123/", "First") +
			CreateTitle("https://www.linkedin.com/jobs/view/456/", "Second") +
			"<p class='line-clamp-1'>Only Company &middot; Tokyo</p>" +
			"</td></tr></table>";

		EmailParseResult result =
			new LinkedInEmailParser().Parse(CreateEmail(html));

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.Candidates, Is.Empty);
	}

	private static Email CreateEmail(string? html)
	{
		Email email = new Email();
		email.SenderAddress = "jobalerts-noreply@linkedin.com";
		email.SentOn = new DateTime(2026, 11, 17, 10, 0, 0);
		email.HtmlBody = html;
		email.TextBody = "Original text";
		return email;
	}

	private static string CreateCard(
		string address, string title, string? details)
	{
		string content = "<table><tbody><tr><td>" +
			"<a href='" + address + "'><img alt='Company'></a>" +
			CreateTitle(address, title);
		if (details != null)
		{
			content += "<p class='line-clamp-1'>" + details + "</p>";
		}

		return content + "</td></tr></tbody></table>";
	}

	private static string CreateTitle(string address, string title)
	{
		return "<a class='font-bold text-md' href='" + address + "'>" +
			title + "</a>";
	}
}
