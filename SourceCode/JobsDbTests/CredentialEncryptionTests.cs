/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialEncryptionTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using DigitalZenWorks.JobsDb.Library;
using NUnit.Framework;

/// <summary>
/// Covers the current credential encryption format.
/// </summary>
[TestFixture]
internal sealed class CredentialEncryptionTests
{
	private const string MasterPassword = "TestPassword123!";

	/// <summary>
	/// Verifies that encryption produces ciphertext from plaintext.
	/// </summary>
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

	/// <summary>
	/// Verifies that a credential round-trips with the same password.
	/// </summary>
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

	/// <summary>
	/// Records the current deterministic ciphertext behavior.
	/// </summary>
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

	/// <summary>
	/// Verifies that decrypting the fixture with a wrong password fails.
	/// </summary>
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

	/// <summary>
	/// Checks that encrypting empty input returns an empty string.
	/// </summary>
	[Test]
	public void Encrypt_EmptyString_ReturnsEmptyString()
	{
		// Act
		var result = CredentialEncryption.Encrypt(string.Empty, MasterPassword);

		// Assert
		Assert.That(result, Is.Empty);
	}

	/// <summary>
	/// Checks that decrypting empty input returns an empty string.
	/// </summary>
	[Test]
	public void Decrypt_EmptyString_ReturnsEmptyString()
	{
		// Act
		var result = CredentialEncryption.Decrypt(string.Empty, MasterPassword);

		// Assert
		Assert.That(result, Is.Empty);
	}
}
