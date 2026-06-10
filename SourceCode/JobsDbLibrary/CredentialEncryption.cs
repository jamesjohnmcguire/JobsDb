/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialEncryption.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Core;

using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Utility class for encrypting/decrypting credentials
/// </summary>
public static class CredentialEncryption
{
	private static readonly byte[] Salt = Encoding.UTF8.GetBytes("JobsDb_Salt_2024_v1");

	/// <summary>
	/// Encrypt a plain text string using AES encryption
	/// </summary>
	public static string Encrypt(string plainText, string masterPassword)
	{
		if (string.IsNullOrEmpty(plainText))
		{
			return string.Empty;
		}

		using Aes aes = Aes.Create();
		Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Salt, 10000, HashAlgorithmName.SHA256);
		aes.Key = key.GetBytes(32);
		aes.IV = key.GetBytes(16);

		using var encryptor = aes.CreateEncryptor();
		var plainBytes = Encoding.UTF8.GetBytes(plainText);
		var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

		return Convert.ToBase64String(encryptedBytes);
	}

	/// <summary>
	/// Decrypt an encrypted string using AES encryption
	/// </summary>
	public static string Decrypt(string encryptedText, string masterPassword)
	{
		if (string.IsNullOrEmpty(encryptedText))
		{
			return string.Empty;
		}

		using Aes aes = Aes.Create();
		Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Salt, 10000, HashAlgorithmName.SHA256);
		aes.Key = key.GetBytes(32);
		aes.IV = key.GetBytes(16);

		using var decryptor = aes.CreateDecryptor();
		var encryptedBytes = Convert.FromBase64String(encryptedText);
		var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

		return Encoding.UTF8.GetString(decryptedBytes);
	}
}
