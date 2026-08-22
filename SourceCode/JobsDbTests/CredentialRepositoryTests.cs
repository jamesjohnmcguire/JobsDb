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
using DigitalZenWorks.JobsDb.Library.Data;
using DigitalZenWorks.JobsDb.Library.Repositories;
using DigitalZenWorks.JobsDb.Library.Scrapers;
using NUnit.Framework;

[TestFixture]
internal class CredentialRepositoryTests
{
	private JobsDbContext context;
	private CredentialRepository repository;
	private string testDbPath;

	[SetUp]
	public void SetUp()
	{
		testDbPath = Path.Combine(Path.GetTempPath(), $"test_creds_{Guid.NewGuid()}.db");
		context = new JobsDbContext(testDbPath);
		context.Database.EnsureCreated();
		repository = new CredentialRepository(context);
	}

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

	[Test]
	public async Task GetBySourceAsync_NonExistingSource_ReturnsNull()
	{
		// Act
		var result = await repository.GetBySourceAsync("NonExistent").ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

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
