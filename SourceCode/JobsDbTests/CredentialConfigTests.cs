/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialConfigTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using JobsDb.Core.Configuration;
using NUnit.Framework;

[TestFixture]
internal class CredentialConfigTests
{
	[Test]
	public void CredentialConfig_SetProperties_ValuesAreSet()
	{
		// Arrange & Act
		CredentialConfig config = new CredentialConfig
		{
			Source = "LinkedIn",
			Username = "user@example.com",
			Password = "MyPassword",
			IsActive = true,
			Notes = "Test credential"
		};

		// Assert
		Assert.That(config.Source, Is.EqualTo("LinkedIn"));
		Assert.That(config.Username, Is.EqualTo("user@example.com"));
		Assert.That(config.Password, Is.EqualTo("MyPassword"));
		Assert.That(config.IsActive, Is.True);
		Assert.That(config.Notes, Is.EqualTo("Test credential"));
	}
}
