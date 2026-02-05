/////////////////////////////////////////////////////////////////////////////
// <copyright file="CredentialRepositoryTests.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using System.Threading.Tasks;
using JobsDb.Core.Data;
using JobsDb.Core.Repositories;
using JobsDbLibrary.Scrapers;
using NUnit.Framework;

[TestFixture]
internal class CredentialRepositoryTests
{
	private JobsDbContext _context;
	private CredentialRepository _repository;
	private string _testDbPath;

	[SetUp]
	public void SetUp()
	{
		_testDbPath = Path.Combine(Path.GetTempPath(), $"test_creds_{Guid.NewGuid()}.db");
		_context = new JobsDbContext(_testDbPath);
		_context.Database.EnsureCreated();
		_repository = new CredentialRepository(_context);
	}

	[TearDown]
	public void TearDown()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();

		if (File.Exists(_testDbPath))
			File.Delete(_testDbPath);
	}

	[Test]
	public async Task AddOrUpdateAsync_NewCredential_AddsToDatabase()
	{
		// Arrange
		var credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "test@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};

		// Act
		var result = await _repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Assert
		Assert.That(result.Id, Is.GreaterThan(0));
		var retrieved = await _repository.GetBySourceAsync("LinkedIn").ConfigureAwait(false);
		Assert.That(retrieved, Is.Not.Null);
	}

	[Test]
	public async Task AddOrUpdateAsync_ExistingCredential_UpdatesInDatabase()
	{
		// Arrange
		var credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "old@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};
		await _repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		credential.Username = "new@example.com";

		// Act
		var result = await _repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Assert
		var retrieved = await _repository.GetBySourceAsync("LinkedIn").ConfigureAwait(false);
		Assert.That(retrieved.Username, Is.EqualTo("new@example.com"));
	}

	[Test]
	public async Task GetBySourceAsync_ExistingCredential_ReturnsCredential()
	{
		// Arrange
		var credential = new ScraperCredential
		{
			CookieData = "cookie",
			Source = "TokyoDev",
			Username = "test@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		};
		await _repository.AddOrUpdateAsync(credential).ConfigureAwait(false);

		// Act
		var result = await _repository.GetBySourceAsync("TokyoDev").ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Source, Is.EqualTo("TokyoDev"));
	}

	[Test]
	public async Task GetBySourceAsync_NonExistingSource_ReturnsNull()
	{
		// Act
		var result = await _repository.GetBySourceAsync("NonExistent").ConfigureAwait(false);

		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public async Task GetAllActiveAsync_OnlyReturnsActive()
	{
		// Arrange
		await _repository.AddOrUpdateAsync(new ScraperCredential
		{
			CookieData = "cookie",
			Source = "LinkedIn",
			Username = "test1@example.com",
			EncryptedPassword = "encrypted",
			IsActive = true
		}).ConfigureAwait(false);
		await _repository.AddOrUpdateAsync(new ScraperCredential
		{
			CookieData = "cookie",
			Source = "TokyoDev",
			Username = "test2@example.com",
			EncryptedPassword = "encrypted",
			IsActive = false
		}).ConfigureAwait(false);

		// Act
		var active = await _repository.GetAllActiveAsync().ConfigureAwait(false);

		// Assert
		Assert.That(active.Count, Is.EqualTo(1));
		Assert.That(active[0].IsActive, Is.True);
	}
}
