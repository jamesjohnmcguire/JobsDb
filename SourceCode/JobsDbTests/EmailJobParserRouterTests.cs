/////////////////////////////////////////////////////////////////////////////
// <copyright file="EmailJobParserRouterTests.cs" company="Digital Zen Works">
// Copyright c 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.Collections.Generic;
using DigitalZenWorks.JobsDb.Library.EmailImport;
using DigitalZenWorks.JobsDb.Library.Models;
using Moq;
using NUnit.Framework;

/// <summary>
/// Covers provider selection and unsupported senders.
/// </summary>
[TestFixture]
internal sealed class EmailJobParserRouterTests
{
	/// <summary>
	/// Selects one provider by sender and preserves its parsing outcome.
	/// </summary>
	[Test]
	public void Parse_SelectsRegisteredParserCaseInsensitively()
	{
		Email email = new Email
		{
			SenderAddress = "JOBS@SECOND.EXAMPLE"
		};
		EmailParseResult expected = new EmailParseResult(
			EmailParseStatus.Ignored,
			false,
			Array.Empty<EmailJobCandidate>(),
			new[] { "Unsupported template." });
		Mock<IEmailJobParser> first = new Mock<IEmailJobParser>();
		Mock<IEmailJobParser> second = new Mock<IEmailJobParser>();
		second.Setup(parser => parser.Parse(email)).Returns(expected);
		Dictionary<string, IEmailJobParser> registrations =
			new Dictionary<string, IEmailJobParser>
			{
				{ "jobs@first.example", first.Object },
				{ "jobs@second.example", second.Object }
			};
		EmailJobParserRouter router = new EmailJobParserRouter(registrations);

		EmailParseResult result = router.Parse(email);

		Assert.That(result, Is.SameAs(expected));
		second.Verify(parser => parser.Parse(email), Times.Once);
		first.VerifyNoOtherCalls();
	}

	/// <summary>
	/// Ignores a sender with no registered parser.
	/// </summary>
	[Test]
	public void Parse_IgnoresUnregisteredSender()
	{
		Email email = new Email
		{
			SenderAddress = "jobs@provider.example.org"
		};
		Mock<IEmailJobParser> parser = new Mock<IEmailJobParser>();
		Dictionary<string, IEmailJobParser> registrations =
			new Dictionary<string, IEmailJobParser>
			{
				{ "jobs@provider.example", parser.Object }
			};
		EmailJobParserRouter router = new EmailJobParserRouter(registrations);

		EmailParseResult result = router.Parse(email);

		Assert.That(result.Status, Is.EqualTo(EmailParseStatus.Ignored));
		Assert.That(result.Candidates, Is.Empty);
		parser.VerifyNoOtherCalls();
	}
}
