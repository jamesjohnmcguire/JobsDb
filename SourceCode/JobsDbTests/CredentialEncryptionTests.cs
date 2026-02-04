/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialEncryptionTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests.Services;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobsDb.Core;
using JobsDb.Core.Data;
using JobsDb.Core.Models;
using JobsDb.Core.Repositories;
using JobsDb.Core.Scrapers;
using JobsDb.Core.Services;
using JobsDbLibrary.Scrapers;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CredentialEncryptionTests
{
	private const string MasterPassword = "TestPassword123!";

	[Test]
	public void Encrypt_ValidText_ReturnsEncryptedString()
	{
		// Arrange
		var plainText = "MySecretPassword";

		// Act
		var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

		// Assert
		Assert.That(encrypted, Is.Not.Null);
		Assert.That(encrypted, Is.Not.Empty);
		Assert.That(encrypted, Is.Not.EqualTo(plainText));
	}

	[Test]
	public void Decrypt_EncryptedText_ReturnsOriginalString()
	{
		// Arrange
		var plainText = "MySecretPassword";
		var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

		// Act
		var decrypted = CredentialEncryption.Decrypt(encrypted, MasterPassword);

		// Assert
		Assert.That(decrypted, Is.EqualTo(plainText));
	}

	[Test]
	public void Encrypt_SameTextTwice_ProducesSameResult()
	{
		// Arrange
		var plainText = "MySecretPassword";

		// Act
		var encrypted1 = CredentialEncryption.Encrypt(plainText, MasterPassword);
		var encrypted2 = CredentialEncryption.Encrypt(plainText, MasterPassword);

		// Assert
		Assert.That(encrypted1, Is.EqualTo(encrypted2));
	}

	[Test]
	public void Decrypt_WrongPassword_ThrowsException()
	{
		// Arrange
		var plainText = "MySecretPassword";
		var encrypted = CredentialEncryption.Encrypt(plainText, MasterPassword);

		// Act & Assert
		Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
		{
			CredentialEncryption.Decrypt(encrypted, "WrongPassword");
		});
	}

	[Test]
	public void Encrypt_EmptyString_ReturnsEmptyString()
	{
		// Act
		var result = CredentialEncryption.Encrypt("", MasterPassword);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void Decrypt_EmptyString_ReturnsEmptyString()
	{
		// Act
		var result = CredentialEncryption.Decrypt("", MasterPassword);

		// Assert
		Assert.That(result, Is.Empty);
	}
}
