/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialRepositoryTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using System.Threading.Tasks;
using DigitalZenWorks.JobsDb.Library;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using NUnit.Framework;

/// <summary>
/// Covers SQLite storage and lookup of source credentials.
/// </summary>
[TestFixture]
internal sealed class CredentialRepositoryTests
{
	private JobsDbContext context;
	private CredentialRepository repository;
	private string testDbPath;

	/// <summary>
	/// Creates a temporary database and credential repository.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_creds_{Guid.NewGuid()}.db");
		DatabaseTestSchema.Create(testDbPath);
		context = new JobsDbContext(testDbPath);
		repository = new CredentialRepository(context);
	}

	/// <summary>
	/// Disposes the context and removes the temporary database.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		context.Database.EnsureDeleted();
		context.Dispose();

		if (File.Exists(testDbPath))
		{
			File.Delete(testDbPath);
		}
	}

	/// <summary>
	/// Verifies that a new source creates a credential record.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task AddOrUpdateAsync_NewCredential_AddsToDatabase()
	{
		// Arrange
		ScraperCredential credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "test@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};

		// Act
		var result = await repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Assert
		Assert.That(result.Id, Is.GreaterThan(0));
		var retrieved = await repository.GetBySourceAsync("LinkedIn").ConfigureAwait(false);
		Assert.That(retrieved, Is.Not.Null);
	}

	/// <summary>
	/// Verifies that saving an existing source updates its record.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task AddOrUpdateAsync_ExistingCredential_UpdatesInDatabase()
	{
		// Arrange
		ScraperCredential credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "old@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};
		await repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		credential.Username = "new@example.com";

		// Act
		var result = await repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Assert
		var retrieved = await repository.GetBySourceAsync("LinkedIn").ConfigureAwait(false);
		Assert.That(retrieved.Username, Is.EqualTo("new@example.com"));
	}

	/// <summary>
	/// Verifies that a known source returns its credential.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetBySourceAsync_ExistingCredential_ReturnsCredential()
	{
		// Arrange
		ScraperCredential credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "TokyoDev",
			Username = "test@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};
		await repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Act
		var result = await repository.GetBySourceAsync("TokyoDev").ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Source, Is.EqualTo("TokyoDev"));
	}

	/// <summary>
	/// Verifies that an unknown source yields no credential.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetBySourceAsync_NonExistingSource_ReturnsNull()
	{
		// Act
		var result = await repository.GetBySourceAsync("NonExistent").ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

	/// <summary>
	/// Verifies that the active query excludes disabled credentials.
	/// </summary>
	/// <returns>A task that completes when the assertions finish.</returns>
	[Test]
	public async Task GetAllActiveAsync_OnlyReturnsActive()
	{
		// Arrange
		await repository.AddOrUpdateAsync(new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "test1@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		}).ConfigureAwait(false);
		await repository.AddOrUpdateAsync(new ScraperCredential
		{
			CookieData = "cookie",
			Source = "TokyoDev",
			Username = "test2@example.com",
			EncryptedPassword = "encrypted",
			IsActive = false
		}).ConfigureAwait(false);

		// Act
		var active = await repository.GetAllActiveAsync().ConfigureAwait(false);

		// Assert
		Assert.That(active.Count, Is.EqualTo(1));
		Assert.That(active[0].IsActive, Is.True);
	}
}
