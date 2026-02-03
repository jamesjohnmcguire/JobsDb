/////////////////////////////////////////////////////////////////////////////
// <copyright file="BaseTestsSupport.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDb.Tests;

using System;
using System.Globalization;
using System.IO;
using JobsDb.Core.Models;
using NUnit.Framework;

/// <summary>
/// Base test support class.
/// </summary>
internal abstract class BaseTestsSupport : IDisposable
{
	private bool disposed;

	/// <summary>
	/// Gets or sets the test database path.
	/// </summary>
	protected string? TestDbPath { get; set; }

	/// <summary>
	/// Gets or sets the test job.
	/// </summary>
	protected Job? TestJob { get; set; }

	/// <summary>
	/// The one time setup method.
	/// </summary>
	[OneTimeSetUp]
	public void BaseOneTimeSetUp()
	{
		TestDbPath =
			Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");

		TestJob = new Job
		{
			Title = "Test",
			Company = "Test Company",
			Description = "Test Description",
			JobType = "Full-time",
			Notes = "Test Notes",
			Location = "Remote",
			Requirements = "None",
			RemoteType = "Remote",
			SalaryCurrency = "USD",
			Source = "Test",
			SourceUrl = "https://test.com",
			SourceJobId = "123",
			DatePosted = DateTime.UtcNow
		};
	}

	/// <summary>
	/// One time tear down method.
	/// </summary>
	[OneTimeTearDown]
	public void BaseOneTimeTearDown()
	{
		if (File.Exists(TestDbPath))
		{
			File.Delete(TestDbPath);
		}
	}

	/// <summary>
	/// Dispose method.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose method.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged
	/// resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing == true && disposed == false)
		{
		}

		disposed = true;
	}
}
